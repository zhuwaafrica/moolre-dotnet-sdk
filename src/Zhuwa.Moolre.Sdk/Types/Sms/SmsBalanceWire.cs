using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of an SMS account status response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class SmsBalanceWire
{
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }
}
