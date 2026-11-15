using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;
namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the send single SMS operation.</summary>
public class SendSingleSmsRequest
{
    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    [JsonPropertyName("senderid")]
    public required string SenderId { get; init; }

    /// <summary>Network. Moolre field <c>network</c>.</summary>
    [JsonIgnore]
    public SmsDeliveryNetwork Network { get; init; }

    // Moolre's "network": sent only when a network was chosen; with the default Moolre delivers over Telecel.
    [JsonInclude]
    [JsonPropertyName("network")]
    internal string? WireNetwork => Network.ToWire(nameof(Network));

    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    [JsonPropertyName("recipient")]
    public required string Recipient { get; init; }

    /// <summary>Text. Moolre field <c>message</c> (max 160 characters).</summary>
    [JsonPropertyName("message")]
    public required string Text { get; init; }
}
