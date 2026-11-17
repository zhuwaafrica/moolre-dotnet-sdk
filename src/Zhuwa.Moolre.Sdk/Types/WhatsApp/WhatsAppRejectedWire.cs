using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>A message Moolre did not send because its reference was not unique. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class WhatsAppRejectedWire
{
    [JsonPropertyName("recipient")]
    public string? Recipient { get; set; }

    [JsonPropertyName("ref")]
    public string? Reference { get; set; }
}
