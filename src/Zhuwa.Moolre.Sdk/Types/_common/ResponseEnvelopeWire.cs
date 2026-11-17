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

    // WhatsApp only: the cursor for the next page of templates.
    [JsonPropertyName("next")]
    public string? Next { get; set; }

    // WhatsApp only: a warning on a send, as text, or an empty array when there is none.
    [JsonPropertyName("error")]
    public System.Text.Json.JsonElement Error { get; set; }
}
