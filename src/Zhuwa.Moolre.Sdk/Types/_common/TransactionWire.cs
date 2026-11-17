using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>Moolre's shared Transaction record. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class TransactionWire
{
    [JsonPropertyName("txstatus")]
    public TransactionStatus Status { get; set; }

    [JsonPropertyName("txtype")]
    public int TransactionType { get; set; }

    [JsonPropertyName("accountnumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("payer")]
    public string? Payer { get; set; }

    [JsonPropertyName("payee")]
    public string? Payee { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    [JsonPropertyName("networkfee")]
    public decimal? NetworkFee { get; set; }

    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("thirdpartyref")]
    public string? ThirdPartyReference { get; set; }

    [JsonPropertyName("ts")]
    public DateTimeOffset Timestamp { get; set; }
}
