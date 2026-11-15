using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>
/// The parts every Moolre response shares. Properties are settable (not init-only) so a field Moolre leaves out keeps its default
/// with the source generator.
/// </summary>
/// <remarks> Internal; field names are declared once, here.</remarks>
internal sealed class ResponseEnvelopeWire
{
    [JsonPropertyName("status")]
    public System.Text.Json.JsonElement Status { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    [JsonConverter(typeof(MessagesConverter))]
    public List<string> Messages { get; set; } = [];

    [JsonPropertyName("data")]
    public System.Text.Json.JsonElement Data { get; set; }
}
