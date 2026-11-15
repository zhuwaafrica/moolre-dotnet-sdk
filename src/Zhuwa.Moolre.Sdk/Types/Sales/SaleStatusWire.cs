using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a sale status response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class SaleStatusWire
{
    [JsonPropertyName("txstatus")]
    public int? Status { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("network")]
    public string? Network { get; set; }

    [JsonPropertyName("bundle")]
    public string? BundleId { get; set; }

    [JsonPropertyName("receiver")]
    public string? Receiver { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("charged")]
    public decimal? Charged { get; set; }

    [JsonPropertyName("accountnumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("refundid")]
    public string? RefundId { get; set; }

    [JsonPropertyName("reference")]
    public string? SaleReference { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("thirdpartyref")]
    public string? ThirdPartyReference { get; set; }

    [JsonPropertyName("ts")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
