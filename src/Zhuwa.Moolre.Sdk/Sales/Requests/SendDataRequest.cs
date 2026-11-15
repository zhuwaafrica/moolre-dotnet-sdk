using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the send data operation.</summary>
public class SendDataRequest
{
    /// <summary>Network. Moolre field <c>channel</c>.</summary>
    [JsonPropertyName("channel")]
    public required SalesNetwork Network { get; init; }

    /// <summary>Receiver phone number. Moolre field <c>receiver</c>.</summary>
    [JsonPropertyName("receiver")]
    public required string ReceiverPhoneNumber { get; init; }

    /// <summary>Bundle ID. Moolre field <c>bundle</c> (an id returned by ListBundlesAsync, passed unchanged).</summary>
    [JsonPropertyName("bundle")]
    public required string BundleId { get; init; }

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

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
