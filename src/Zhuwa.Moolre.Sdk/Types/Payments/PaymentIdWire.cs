using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a Payment ID response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class PaymentIdWire
{
    [JsonPropertyName("paymentid")]
    public string? PaymentId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("qrcode")]
    public string? QrCode { get; set; }
}
