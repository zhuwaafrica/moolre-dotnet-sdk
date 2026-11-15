using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the send airtime operation.</summary>
public class SendAirtimeRequest
{
    /// <summary>Network. Moolre field <c>channel</c>.</summary>
    [JsonPropertyName("channel")]
    public required SalesNetwork Network { get; init; }

    /// <summary>Receiver phone number. Moolre field <c>receiver</c>.</summary>
    [JsonPropertyName("receiver")]
    public required string ReceiverPhoneNumber { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string, GHS, minimum 1.00 — enforced by Moolre).</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalAsTextConverter))]
    public required decimal Amount { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (unique per wallet).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    [JsonPropertyName("reference")]
    public string? Narration { get; init; }

    // Moolre sells airtime and data in GHS only and requires the field, so it is always sent and not exposed.
    [JsonInclude]
    [JsonPropertyName("currency")]
    internal string WireCurrency { get; } = "GHS";

    /// <summary>Account number. Moolre field <c>accountnumber</c> (must be linked to the Sell on Our Platform service).</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
