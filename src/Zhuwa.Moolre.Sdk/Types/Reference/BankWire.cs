using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One bank in the bank list. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BankWire
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
