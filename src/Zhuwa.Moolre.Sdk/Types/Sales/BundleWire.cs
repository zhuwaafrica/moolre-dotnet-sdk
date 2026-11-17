using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One data bundle in the bundle list. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BundleWire
{
    [JsonPropertyName("bundle")]
    public string? BundleId { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("data")]
    public string? Label { get; set; }

    [JsonPropertyName("data_mb")]
    public double Megabytes { get; set; }

    [JsonPropertyName("validity")]
    public string? Validity { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }
}
