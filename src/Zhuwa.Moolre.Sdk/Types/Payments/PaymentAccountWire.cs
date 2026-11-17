using System.Text.Json.Serialization;

namespace Zhuwa.Moolre;

/// <summary>The account summary in a Payment ID or bank account lookup. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class PaymentAccountWire
{
    [JsonPropertyName("accountno")]
    public string? BankAccountNumber { get; set; }

    [JsonPropertyName("txcount")]
    public int TransactionCount { get; set; }

    [JsonPropertyName("txvalue")]
    public decimal TransactionValue { get; set; }
}
