using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a payment callback. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class PaymentWebhookWire
{
    [JsonPropertyName("txstatus")]
    public TransactionStatus Status { get; set; }

    [JsonPropertyName("payer")]
    public string? Payer { get; set; }

    [JsonPropertyName("payername")]
    public string? PayerName { get; set; }

    [JsonPropertyName("payernetwork")]
    public string? PayerNetwork { get; set; }

    [JsonPropertyName("payerinstitution")]
    public string? PayerInstitution { get; set; }

    [JsonPropertyName("terminalid")]
    public string? TerminalId { get; set; }

    [JsonPropertyName("accountnumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("thirdpartyref")]
    public string? ThirdPartyReference { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonPropertyName("ts")]
    public DateTimeOffset Timestamp { get; set; }
}
