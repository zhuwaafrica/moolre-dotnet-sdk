using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>One account in the bank account list. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BankAccountItemWire
{
    [JsonPropertyName("accountno")]
    public string? BankAccountNumber { get; set; }

    [JsonPropertyName("accountname")]
    public string? AccountName { get; set; }

    [JsonPropertyName("bankname")]
    public string? BankName { get; set; }

    [JsonPropertyName("extid")]
    public string? ExternalReference { get; set; }

    [JsonPropertyName("ts")]
    public DateTimeOffset CreatedAt { get; set; }
}
