using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One message in an SMS status response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class SmsStatusWire
{
    [JsonPropertyName("ref")]
    public string? Reference { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}
