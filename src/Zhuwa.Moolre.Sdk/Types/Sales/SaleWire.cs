using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of an airtime or data sale response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class SaleWire
{
    [JsonPropertyName("txstatus")]
    public TransactionStatus Status { get; set; }

    [JsonPropertyName("receiver")]
    public string? Receiver { get; set; }

    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
