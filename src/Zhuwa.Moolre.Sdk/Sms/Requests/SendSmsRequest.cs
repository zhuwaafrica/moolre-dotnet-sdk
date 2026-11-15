using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the send SMS operation.</summary>
public class SendSmsRequest
{
    /// <summary>Sender ID. Moolre field <c>senderid</c> (max 11 characters, must be approved).</summary>
    [JsonPropertyName("senderid")]
    public required string SenderId { get; init; }

    /// <summary>Network. Moolre field <c>network</c> (live only).</summary>
    [JsonIgnore]
    public SmsDeliveryNetwork Network { get; init; }

    // Moolre's "network": sent only when a network was chosen; with the default Moolre delivers over Telecel.
    [JsonInclude]
    [JsonPropertyName("network")]
    internal string? WireNetwork => Network.ToWire(nameof(Network));

    /// <summary>Messages. Moolre field <c>messages</c> (initialised empty).</summary>
    [JsonPropertyName("messages")]
    public Collection<SmsMessage> Messages { get; } = [];
}
