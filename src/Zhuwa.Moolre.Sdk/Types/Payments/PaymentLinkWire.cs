using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a payment link response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class PaymentLinkWire
{
    [JsonPropertyName("authorization_url")]
    public Uri? AuthorizationUrl { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }
}
