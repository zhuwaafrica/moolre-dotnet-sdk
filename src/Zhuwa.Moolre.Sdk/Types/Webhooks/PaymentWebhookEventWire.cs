using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>
/// The top-level event fields of a callback, next to the envelope's <c>status</c>, <c>code</c> and <c>data</c>. Internal: the mapper turns it
/// into the public result. Field names are declared once, here.
/// </summary>
internal sealed class PaymentWebhookEventWire
{
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    [JsonPropertyName("event")]
    public string? EventType { get; set; }
}
