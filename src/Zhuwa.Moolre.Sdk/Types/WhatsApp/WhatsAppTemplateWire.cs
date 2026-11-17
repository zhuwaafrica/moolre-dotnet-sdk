using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One template in a WhatsApp templates response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class WhatsAppTemplateWire
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Body { get; set; }

    [JsonPropertyName("placeholders")]
    public List<string?> Placeholders { get; set; } = [];
}
