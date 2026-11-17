using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The data of a created bank account response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BankAccountWire
{
    [JsonPropertyName("accountno")]
    public string? BankAccountNumber { get; set; }

    [JsonPropertyName("accountname")]
    public string? AccountName { get; set; }

    [JsonPropertyName("bankname")]
    public string? BankName { get; set; }

    [JsonPropertyName("externalref")]
    public string? ExternalReference { get; set; }
}
