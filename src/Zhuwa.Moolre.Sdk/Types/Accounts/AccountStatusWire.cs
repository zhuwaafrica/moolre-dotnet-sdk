using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of an account status response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class AccountStatusWire
{
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("accountname")]
    public string? AccountName { get; set; }

    [JsonPropertyName("activated")]
    public bool IsActivated { get; set; }

    [JsonPropertyName("callback")]
    public Uri? CallbackUrl { get; set; }
}
