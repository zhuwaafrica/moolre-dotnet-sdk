using System.Buffers;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zhuwa.Moolre.Internal.Serialization;

namespace Zhuwa.Moolre.Internal.Transport;

/// <summary>
/// Sends every Moolre call: builds the request from the endpoint table, adds the credentials, applies the timeout,
/// reads the response and turns Moolre's failures into exceptions. Calls that move money or send messages are never retried.
/// </summary>
internal sealed partial class MoolreTransport
{
    // Codes that mean the credentials or the wallet were not accepted.
    private static readonly HashSet<string> AuthenticationCodes = new(StringComparer.Ordinal)
    {
        "AIN01", "AIN03", "AIN04", "IN01", "ASL00", "ASS00",
    };

    // Codes that mean the external reference was already used.
    private static readonly HashSet<string> DuplicateReferenceCodes = new(StringComparer.Ordinal)
    {
        "TP13", "TR013", "APY18", "ASL08", "INP02",
    };

    // Creates a fresh HttpClient per call; the factory pools and recycles the handlers underneath (DNS changes, socket reuse).
    private readonly IHttpClientFactory _httpClientFactory;

    // A monitor, not a snapshot: every call reads the current values, so a changed key or timeout applies without a restart.
    private readonly IOptionsMonitor<MoolreOptions> _options;

    private readonly ILogger<MoolreTransport> _logger;

    public MoolreTransport(IHttpClientFactory httpClientFactory, IOptionsMonitor<MoolreOptions> options, ILogger<MoolreTransport> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    /// <summary>The wallet to use when a request does not name one.</summary>
    internal string? DefaultAccountNumber => _options.CurrentValue.AccountNumber;

    /// <summary>Sends one call and returns Moolre's answer, or throws for authentication, duplicate reference and unreadable answers.</summary>
    /// <param name="endpoint">The operation, from <see cref="MoolreEndpoints"/>.</param>
    /// <param name="writeFields">Writes the request fields into the JSON body (the <c>type</c> field is added for you), or <see langword="null"/>.</param>
    /// <param name="query">The query string for a GET call, without the leading <c>?</c>.</param>
    /// <param name="externalReference">The caller's reference, carried by <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <param name="cancellationToken">
    /// Cancels the call. Cancelling before anything is sent throws <see cref="OperationCanceledException"/>. Cancelling a call that
    /// moves money or sends a message after it started throws <see cref="MoolreOutcomeUncertainException"/>, because it may have been processed.
    /// </param>
    internal async Task<ResponseEnvelope> SendAsync(
        MoolreEndpoint endpoint,
        Action<Utf8JsonWriter>? writeFields,
        string? query,
        string? externalReference,
        CancellationToken cancellationToken)
    {
        // Nothing has been sent yet, so a token that is already cancelled is a plain cancellation.
        cancellationToken.ThrowIfCancellationRequested();

        // Read once so the credentials, the timeout and the error text in this call all come from the same snapshot.
        var options = _options.CurrentValue;

        // Build the request and add the credentials before any network use: a missing or invalid credential throws
        // InvalidOperationException here, so nothing has been sent when it does.
        using var request = CreateRequest(endpoint, writeFields, query);
        MoolreRequestPolicy.ApplyCredentials(request.Headers, endpoint, options);

        // One token for both the caller's cancellation and our own timeout. The HttpClient timeout is switched off
        // (see MoolreHttpClientConfiguration), so this is the only timeout, and we can tell the two causes apart below.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.Timeout);

        HttpStatusCode statusCode;
        byte[] body;
        try
        {
            // The HttpClient is not disposed: clients from IHttpClientFactory are cheap and must not be disposed per call.
            var client = _httpClientFactory.CreateClient(MoolreHttpClientConfiguration.ClientName);
            // Headers first, then the body inside the same try: a failure while reading the body must be handled the same way
            // as a failure to connect, because the request has already been sent by then.
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            statusCode = response.StatusCode;
            body = await response.Content.ReadAsByteArrayAsync(timeout.Token).ConfigureAwait(false);
        }
        // Cancelled, but not by the caller: this is our own timeout.
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            LogTimedOut(_logger, endpoint.Name);
            // A read-only call can simply be repeated, so a plain TimeoutException is enough. A call that moves money or
            // sends a message may have been processed, so the caller is told the outcome is unknown and given the reference.
            throw endpoint.IsSafeToRepeat
                ? new TimeoutException($"{endpoint.Name} timed out after {options.Timeout}.", ex)
                : Uncertain(endpoint, externalReference, "timed out", ex);
        }
        catch (OperationCanceledException ex) when (!endpoint.IsSafeToRepeat)
        {
            // The caller cancelled after the request was started, so it may already have reached Moolre.
            throw Uncertain(endpoint, externalReference, "was cancelled after it was sent", ex);
        }
        // Connection and read failures. For read-only calls these pass through unchanged so the caller can retry.
        // The filter is narrow on purpose: no other exception type is swallowed.
        catch (Exception ex) when ((ex is HttpRequestException or IOException) && !endpoint.IsSafeToRepeat)
        {
            LogFailed(_logger, endpoint.Name, ex);
            throw Uncertain(endpoint, externalReference, "failed after the request may have been sent", ex);
        }

        // An answer arrived. Read it into the shared envelope; a body that is not Moolre JSON is a failure
        // (and, for a money or message call, an unknown outcome, because the server may have acted before replying badly).
        ResponseEnvelope envelope;
        try
        {
            envelope = ResponseEnvelope.Parse(body, statusCode);
        }
        catch (MoolreResponseException ex) when (!endpoint.IsSafeToRepeat)
        {
            // Moolre answered, but not with something we can read: whether the money or message went through is unknown.
            throw Uncertain(endpoint, externalReference, "returned a response that could not be read", ex);
        }

        // Log the operation, HTTP status and Moolre code only. Never the body, headers or credentials.
        LogCompleted(_logger, endpoint.Name, (int)statusCode, envelope.Code.Value);

        // Authentication and duplicate-reference failures mean the same thing for every operation, so they are handled here.
        // Any other failure is left to the caller, because some "failures" (pending, OTP needed, not found) are valid results.
        ThrowForKnownFailures(envelope);
        return envelope;
    }

    /// <summary>
    /// Throws <see cref="MoolreApiException"/> when Moolre did not report success. Operations call this when status 0 is a
    /// real failure; operations where status 0 can be a valid answer (for example "name not found") check the code instead.
    /// </summary>
    internal static void EnsureSuccess(ResponseEnvelope envelope)
    {
        if (!envelope.IsSuccess)
        {
            throw new MoolreApiException(
                envelope.Message ?? "Moolre reported a failure.",
                envelope.Code,
                envelope.HttpStatusCode,
                envelope.Messages,
                envelope.Detail);
        }
    }

    private static HttpRequestMessage CreateRequest(MoolreEndpoint endpoint, Action<Utf8JsonWriter>? writeFields, string? query)
    {
        // The path is relative: the base address (live or sandbox) is set once on the HttpClient.
        var path = string.IsNullOrEmpty(query) ? endpoint.Path : $"{endpoint.Path}?{query}";
        var request = new HttpRequestMessage(endpoint.Method, new Uri(path, UriKind.Relative));

        // GET calls carry their values in the query string and have no body.
        if (endpoint.Method != HttpMethod.Get)
        {
            // Written straight to UTF-8 with Utf8JsonWriter: no reflection (trimming and AOT safe) and correct escaping of every value.
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                // Many Moolre operations share one URL and differ only by this field, so it comes from the endpoint table.
                if (endpoint.Type is int type)
                {
                    writer.WriteNumber("type", type);
                }

                writeFields?.Invoke(writer);
                writer.WriteEndObject();
            }

            // A byte array, not a stream: it can be sent again by the HttpClient if it ever needs to (for example a handler the
            // caller adds), and the request disposes it with the message.
            request.Content = new ByteArrayContent(buffer.WrittenSpan.ToArray());
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        return request;
    }

    private static void ThrowForKnownFailures(ResponseEnvelope envelope)
    {
        // Compared as plain text so a code Moolre adds later is never mistaken for one of these.
        var code = envelope.Code.Value;
        var message = envelope.Message ?? "Moolre rejected the call.";

        if (AuthenticationCodes.Contains(code))
        {
            throw new MoolreAuthenticationException(message, envelope.Code, envelope.HttpStatusCode, envelope.Messages, envelope.Detail);
        }

        if (DuplicateReferenceCodes.Contains(code))
        {
            throw new MoolreDuplicateReferenceException(message, envelope.Code, envelope.HttpStatusCode, envelope.Messages, envelope.Detail);
        }
    }

    private static MoolreOutcomeUncertainException Uncertain(MoolreEndpoint endpoint, string? externalReference, string what, Exception inner) =>
        new(
            $"{endpoint.Name} {what}. It may or may not have been processed; check its status before doing anything else.",
            endpoint.Name,
            externalReference,
            inner);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Moolre {Operation} answered HTTP {HttpStatus} with code {Code}")]
    private static partial void LogCompleted(ILogger logger, string operation, int httpStatus, string code);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Moolre {Operation} timed out")]
    private static partial void LogTimedOut(ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Moolre {Operation} failed before an answer was read")]
    private static partial void LogFailed(ILogger logger, string operation, Exception exception);
}
