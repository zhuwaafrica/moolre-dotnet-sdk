using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>A sender ID in a status, list or approve response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class SenderIdWire
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("senderid")]
    public string? SenderId { get; set; }

    [JsonPropertyName("approval")]
    public string? Approval { get; set; }

    [JsonPropertyName("whitelisted")]
    public bool IsWhitelisted { get; set; }
}
