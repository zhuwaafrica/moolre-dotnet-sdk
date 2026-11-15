using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>The data of a create or update account response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class AccountWire
{
    [JsonPropertyName("status")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("accountnumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("accountname")]
    public string? AccountName { get; set; }

    [JsonPropertyName("paymentid")]
    public string? PaymentId { get; set; }

    [JsonPropertyName("api")]
    public bool ApiEnabled { get; set; }

    [JsonPropertyName("callback")]
    public Uri? CallbackUrl { get; set; }

    [JsonPropertyName("settlement")]
    [JsonConverter(typeof(LenientObjectConverter<SettlementSettings>))]
    public SettlementSettings? Settlement { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}
