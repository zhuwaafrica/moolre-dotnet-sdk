using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>The data of a bundle list response: one list per network. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BundleCatalogWire
{
    [JsonPropertyName("mtn")]
    [JsonConverter(typeof(ObjectListConverter<BundleWire>))]
    public List<BundleWire> Mtn { get; set; } = [];

    [JsonPropertyName("telecel")]
    [JsonConverter(typeof(ObjectListConverter<BundleWire>))]
    public List<BundleWire> Telecel { get; set; } = [];

    [JsonPropertyName("at")]
    [JsonConverter(typeof(ObjectListConverter<BundleWire>))]
    public List<BundleWire> At { get; set; } = [];
}
