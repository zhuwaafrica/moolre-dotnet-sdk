using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One message in a WhatsApp status response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class WhatsAppStatusWire
{
    [JsonPropertyName("ref")]
    public string? Reference { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
