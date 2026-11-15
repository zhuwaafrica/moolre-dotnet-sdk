using System.Net;
using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Webhooks;

/// <summary>Turns the body Moolre posts to your callback URL into a <see cref="PaymentWebhook"/> (static, hand-written, no reflection).</summary>
internal static class WebhooksMapper
{
    /// <summary>Reads a payment callback body.</summary>
    /// <exception cref="MoolreResponseException">The body is not JSON, not a Moolre envelope, or has no <c>data</c> object.</exception>
    internal static PaymentWebhook ToPaymentWebhook(ReadOnlySpan<byte> utf8Json)
    {
        // The same envelope reader as API responses; there is no HTTP status for a body we received, so it is reported as 0.
        var envelope = ResponseEnvelope.Parse(utf8Json, default(HttpStatusCode));
        var data = envelope.Data;

        // Without data there is no reference, amount or secret, so the callback cannot be matched or verified.
        if (data.ValueKind != JsonValueKind.Object)
        {
            throw new MoolreResponseException("The Moolre callback has no data object.");
        }

        var details = new PaymentWebhookData(
            data.ReadTransactionStatus("txstatus"),
            data.ReadString("payer") ?? string.Empty,
            // Added by Moolre on 11 Nov 2026 and absent from failed callbacks: an empty value reads as null, not as a name.
            Blank(data.ReadString("payername")),
            Blank(data.ReadString("payernetwork")),
            Blank(data.ReadString("payerinstitution")),
            data.ReadString("terminalid") ?? string.Empty,
            data.ReadString("accountnumber") ?? string.Empty,
            data.ReadString("name") ?? string.Empty,
            data.ReadDecimal("amount"),
            data.ReadDecimal("value"),
            // Null on failed attempts.
            Blank(data.ReadString("transactionid")),
            data.ReadString("externalref") ?? string.Empty,
            data.ReadString("thirdpartyref") ?? string.Empty,
            Blank(data.ReadString("reason")),
            // A missing secret becomes empty, which never matches a configured secret.
            data.ReadString("secret") ?? string.Empty,
            data.ReadTimestamp("ts"));

        return new PaymentWebhook(envelope.Code, envelope.Message, details, envelope.IsSuccess);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
