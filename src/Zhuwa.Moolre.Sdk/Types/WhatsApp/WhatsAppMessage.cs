using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One WhatsApp message: the recipient, the values for the template's placeholders, and an optional reference.</summary>
public class WhatsAppMessage
{
    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    [JsonPropertyName("recipient")]
    public required string Recipient { get; init; }

    /// <summary>Your reference for the message. Moolre field <c>ref</c>. Without it the delivery status cannot be looked up later; it must be unique.</summary>
    [JsonPropertyName("ref")]
    public string? Reference { get; init; }

    /// <summary>The values for the template's placeholders, in order: the first fills <c>{{1}}</c>, the second <c>{{2}}</c>, and so on. Moolre field <c>placeholders</c>.</summary>
    [JsonPropertyName("placeholders")]
    public Collection<string> Placeholders { get; } = [];
}
