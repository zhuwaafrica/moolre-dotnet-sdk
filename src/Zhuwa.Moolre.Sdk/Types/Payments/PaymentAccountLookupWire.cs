using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>The data of a Payment ID or bank account lookup. Internal: the mapper turns it into the public result. Field names are declared once, here.</summary>
internal sealed class PaymentAccountLookupWire
{
    [JsonPropertyName("account")]
    [JsonConverter(typeof(LenientObjectConverter<PaymentAccountWire>))]
    public PaymentAccountWire? Account { get; set; }

    [JsonPropertyName("transactions")]
    [JsonConverter(typeof(OneOrManyConverter<TransactionWire>))]
    public List<TransactionWire> Transactions { get; set; } = [];
}
