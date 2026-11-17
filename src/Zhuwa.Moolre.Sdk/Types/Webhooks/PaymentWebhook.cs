using System.Diagnostics.CodeAnalysis;
using System.Text;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre;

/// <summary>A payment notification that Moolre sends to your callback URL.</summary>
public class PaymentWebhook
{
    private readonly bool _wireStatusIsSuccess;

    internal PaymentWebhook(
        MoolreResponseCode code,
        string? message,
        PaymentWebhookData data,
        bool wireStatusIsSuccess,
        string? eventId,
        string? eventType)
    {
        Code = code;
        Message = message;
        Data = data;
        _wireStatusIsSuccess = wireStatusIsSuccess;
        EventId = eventId;
        EventType = eventType;
    }

    /// <summary>
    /// The event's ID, such as <c>evt_0123456789abcdef01234567</c>. Moolre field <c>event_id</c>. It is the same on every retry and resend,
    /// so store it under a unique key and handle each event once. <see langword="null"/> on callbacks without event fields.
    /// </summary>
    public string? EventId { get; }

    /// <summary>
    /// The event name, such as <c>payment.succeeded</c>, <c>payment.failed</c> or <c>webhook.test</c>. Moolre field <c>event</c>.
    /// <see langword="null"/> on callbacks without event fields.
    /// </summary>
    public string? EventType { get; }

    /// <summary>The payment outcome code: <c>P01</c> successful or <c>P02</c> failed. Not an HTTP status. Moolre field <c>code</c>.</summary>
    public MoolreResponseCode Code { get; }

    /// <summary>Moolre's outcome message, such as "Transaction Successful". Moolre field <c>message</c>.</summary>
    public string? Message { get; }

    /// <summary>The transaction details. Moolre field <c>data</c>.</summary>
    public PaymentWebhookData Data { get; }

    /// <summary>
    /// Whether this is a successful payment: status 1, code <c>P01</c> and <c>txstatus</c> 1, all three.
    /// A failed callback (<c>P02</c>) must never be used to fulfil an order.
    /// </summary>
    public bool IsSuccessfulPayment =>
        _wireStatusIsSuccess
        && Code == MoolreResponseCode.PaymentSucceeded
        && Data.Status == TransactionStatus.Successful;

    /// <summary>
    /// Reads the body Moolre posted to your callback URL. Parsing does not verify it: check the signature with
    /// <see cref="MoolreWebhookVerifier.IsSignatureValid(ReadOnlySpan{byte}, string)"/> before changing any records.
    /// </summary>
    /// <param name="utf8Json">The raw request body.</param>
    /// <returns>The callback.</returns>
    /// <exception cref="MoolreResponseException">The body is not a Moolre payment callback.</exception>
    public static PaymentWebhook Parse(ReadOnlySpan<byte> utf8Json) => WebhooksMapper.ToPaymentWebhook(utf8Json);

    /// <summary>Reads the body Moolre posted to your callback URL, given as text. See <see cref="Parse(ReadOnlySpan{byte})"/>.</summary>
    /// <param name="json">The raw request body.</param>
    /// <returns>The callback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="MoolreResponseException">The body is not a Moolre payment callback.</exception>
    public static PaymentWebhook Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return Parse(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>Tries to read a callback body; returns <see langword="false"/> instead of throwing when it is not a Moolre callback.</summary>
    /// <param name="utf8Json">The raw request body.</param>
    /// <param name="webhook">The callback, when the body could be read.</param>
    /// <returns>Whether the body could be read.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Json, [NotNullWhen(true)] out PaymentWebhook? webhook)
    {
        try
        {
            webhook = Parse(utf8Json);
            return true;
        }
        catch (MoolreResponseException)
        {
            // Only "not a Moolre callback" is turned into false; any other exception is a real bug and must surface.
            webhook = null;
            return false;
        }
    }

    /// <summary>Tries to read a callback body given as text. See <see cref="TryParse(ReadOnlySpan{byte}, out PaymentWebhook)"/>.</summary>
    /// <param name="json">The raw request body; <see langword="null"/> gives <see langword="false"/>.</param>
    /// <param name="webhook">The callback, when the body could be read.</param>
    /// <returns>Whether the body could be read.</returns>
    public static bool TryParse(string? json, [NotNullWhen(true)] out PaymentWebhook? webhook)
    {
        if (json is null)
        {
            webhook = null;
            return false;
        }

        return TryParse(Encoding.UTF8.GetBytes(json), out webhook);
    }
}
