using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>The data of a transaction list response. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class AccountTransactionsWire
{
    [JsonPropertyName("txcount")]
    public int? TransactionCount { get; set; }

    [JsonPropertyName("transactions")]
    [JsonConverter(typeof(ObjectListConverter<TransactionWire>))]
    public List<TransactionWire> Transactions { get; set; } = [];
}
