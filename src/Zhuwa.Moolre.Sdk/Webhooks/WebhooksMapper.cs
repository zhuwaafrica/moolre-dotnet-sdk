using System.Net;
using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Webhooks;

/// <summary>Turns the body Moolre posts to your callback URL into a <see cref="PaymentWebhook"/>. Field names live on <see cref="PaymentWebhookWire"/>.</summary>
internal static class WebhooksMapper
{
    /// <summary>Reads a payment callback body.</summary>
    /// <exception cref="MoolreResponseException">The body is not JSON, not a Moolre envelope, or has no <c>data</c> object.</exception>
    internal static PaymentWebhook ToPaymentWebhook(ReadOnlySpan<byte> utf8Json)
    {
        // The same envelope reader as API responses; there is no HTTP status for a body we received, so it is reported as 0.
        var envelope = ResponseEnvelope.Parse(utf8Json, default(HttpStatusCode));

        // Without data there is no reference, amount or secret, so the callback cannot be matched or verified.
        if (envelope.Data.ValueKind != JsonValueKind.Object)
        {
            throw new MoolreResponseException("The Moolre callback has no data object.");
        }

        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.PaymentWebhookWire, envelope.HttpStatusCode);

        var details = new PaymentWebhookData(
            wire.Status,
            wire.Payer ?? string.Empty,
            // Added by Moolre on 11 Nov 2026 and absent from failed callbacks: an empty value reads as null, not as a name.
            Blank(wire.PayerName),
            Blank(wire.PayerNetwork),
            Blank(wire.PayerInstitution),
            wire.TerminalId ?? string.Empty,
            wire.AccountNumber ?? string.Empty,
            wire.Name ?? string.Empty,
            wire.Amount,
            wire.Value,
            // Null on failed attempts.
            Blank(wire.TransactionId),
            wire.ExternalReference ?? string.Empty,
            wire.ThirdPartyReference ?? string.Empty,
            Blank(wire.Reason),
            // A missing secret becomes empty, which never matches a configured secret.
            wire.Secret ?? string.Empty,
            wire.Timestamp);

        // The event fields sit at the top level, beside the envelope, so they are read from the whole body. The envelope parse above has
        // already proved the body is a JSON object, so this read cannot fail on the shape.
        var eventWire = JsonSerializer.Deserialize(utf8Json, MoolreJsonContext.Default.PaymentWebhookEventWire);

        return new PaymentWebhook(
            envelope.Code,
            envelope.Message,
            details,
            envelope.IsSuccess,
            // Absent from callbacks sent before Moolre added events, so both read as null rather than empty text.
            Blank(eventWire?.EventId),
            Blank(eventWire?.EventType));
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
