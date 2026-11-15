using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>The data of a bank account list response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class BankAccountListWire
{
    [JsonPropertyName("txcount")]
    public int? Count { get; set; }

    [JsonPropertyName("accounts")]
    [JsonConverter(typeof(ObjectListConverter<BankAccountItemWire>))]
    public List<BankAccountItemWire> Accounts { get; set; } = [];
}
