using System.Text.Json.Nodes;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sms;

/// <summary>
/// Sends the SMS calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it through the
/// transport and decides which answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolreSms(MoolreTransport transport) : MoolreSms
{
    // Moolre's documented limits: a sender ID is at most 11 characters, and the GET send takes at most 160 characters of text.
    private const int MaxSenderIdLength = 11;
    private const int MaxSingleMessageLength = 160;

    public override async Task<SmsSendResult> SendAsync(SendSmsRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        // A bulk send has no single reference; an unknown outcome is still reported, and the caller checks the messages' own references.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendSmsRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.Send, body, externalReference: null, cancellationToken).ConfigureAwait(false);

        // An unapproved sender ID (ASMS07) arrives with status 0 and becomes MoolreApiException; nothing was sent.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSmsSendResult();
    }

    public override async Task<SmsSendResult> SendSingleAsync(SendSingleSmsRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        // A GET call: the fields (with the type) become the query string, each value escaped. The SMS key goes in a header, never the URL.
        var query = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendSingleSmsRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.SendSingle, query, externalReference: null, cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSmsSendResult();
    }

    public override async Task<SmsStatusResult> GetStatusAsync(GetSmsStatusRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetSmsStatusRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.GetStatus, body, externalReference: null, cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSmsStatusResult();
    }

    public override async Task<SenderIdsCreatedResult> CreateSenderIdsAsync(CreateSenderIdsRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.CreateSenderIdsRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.CreateSenderIds, body, externalReference: null, cancellationToken).ConfigureAwait(false);

        // No permission to approve (ASMQ09), a missing or too long sender ID (ASMQ06, ASMQ08) arrive with status 0.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSenderIdsCreatedResult();
    }

    public override async Task<SenderIdStatusResult> GetSenderIdStatusAsync(GetSenderIdStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SenderId);

        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetSenderIdStatusRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.GetSenderIdStatus, body, externalReference: null, cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSenderIdStatusResult();
    }

    public override async Task<SenderIdListResult> ListSenderIdsAsync(CancellationToken cancellationToken)
    {
        // Only the type selects this operation; there are no other fields.
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.ListSenderIds, new JsonObject(), externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSenderIdListResult();
    }

    public override async Task<SenderIdListResult> ApproveSenderIdsAsync(ApproveSenderIdsRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.ApproveSenderIdsRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.ApproveSenderIds, body, externalReference: null, cancellationToken).ConfigureAwait(false);

        // Without administrative permission Moolre answers ASMQ09 with status 0.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSenderIdListResult();
    }

    public override async Task<SmsBalanceResult> GetBalanceAsync(CancellationToken cancellationToken)
    {
        // Only the type selects this operation; there are no other fields.
        var envelope = await transport.SendAsync(MoolreEndpoints.Sms.GetBalance, new JsonObject(), externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSmsBalanceResult();
    }

    // Checks a bulk send: an approved-length sender ID, a known network, and at least one message, each with a recipient and text.
    private static void Validate(SendSmsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalidSenderId(request.SenderId, nameof(request.SenderId));
        _ = request.Network.ToWire(nameof(request.Network));
        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("Add at least one message.", nameof(request));
        }

        foreach (var message in request.Messages)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(message.Recipient, nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(message.Text, nameof(request));
        }
    }

    // Checks a single send: sender ID, network, recipient, and text within Moolre's 160-character limit for the GET form.
    private static void Validate(SendSingleSmsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalidSenderId(request.SenderId, nameof(request.SenderId));
        _ = request.Network.ToWire(nameof(request.Network));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Text);
        if (request.Text.Trim().Length > MaxSingleMessageLength)
        {
            throw new ArgumentException($"The text must be at most {MaxSingleMessageLength} characters for a single send. Use SendAsync for longer messages.", nameof(request));
        }
    }

    // Checks a status lookup: at least one reference, none blank.
    private static void Validate(GetSmsStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.References.Count == 0 || request.References.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Add at least one message reference, none blank.", nameof(request));
        }
    }

    // Checks a sender ID registration: at least one, each a valid sender ID.
    private static void Validate(CreateSenderIdsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.SenderIds.Count == 0)
        {
            throw new ArgumentException("Add at least one sender ID.", nameof(request));
        }

        foreach (var registration in request.SenderIds)
        {
            ArgumentNullException.ThrowIfNull(registration, nameof(request));
            ThrowIfInvalidSenderId(registration.SenderId, nameof(request));
        }
    }

    // Checks sender ID decisions: at least one, each with a sender ID and a known decision.
    private static void Validate(ApproveSenderIdsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Decisions.Count == 0)
        {
            throw new ArgumentException("Add at least one decision.", nameof(request));
        }

        foreach (var decision in request.Decisions)
        {
            ArgumentNullException.ThrowIfNull(decision, nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(decision.SenderId, nameof(request));
            _ = decision.Decision.ToWire(nameof(request));
        }
    }

    // Moolre rejects sender IDs over 11 characters; refusing here gives a clear error before anything is sent.
    private static void ThrowIfInvalidSenderId(string senderId, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senderId, paramName);
        if (senderId.Trim().Length > MaxSenderIdLength)
        {
            throw new ArgumentException($"A sender ID must be at most {MaxSenderIdLength} characters.", paramName);
        }
    }
}
