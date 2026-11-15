using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a send transfer response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class TransferWire
{
    [JsonPropertyName("txstatus")]
    public TransactionStatus Status { get; set; }

    [JsonPropertyName("receiver")]
    public string? Receiver { get; set; }

    [JsonPropertyName("transactionid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("thirdpartyref")]
    public string? ThirdPartyReference { get; set; }

    [JsonPropertyName("receivername")]
    public string? ReceiverName { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("amountfee")]
    public decimal AmountWithFee { get; set; }

    [JsonPropertyName("networkfee")]
    public decimal NetworkFee { get; set; }

    [JsonPropertyName("fee")]
    public decimal Fee { get; set; }
}
