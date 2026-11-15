using System.Text.Json;
using Zhuwa.Moolre.Internal.Serialization;

namespace Zhuwa.Moolre.Reference;

/// <summary>Turns Moolre's reference data responses into the public result types (static, hand-written, no reflection).</summary>
internal static class ReferenceDataMapper
{
    /// <summary>Maps a bank list response, whose <c>data</c> is an array of <c>{ name, code }</c>.</summary>
    /// <param name="envelope">The parsed response. A <c>data</c> value that is not an array gives an empty list, not an error.</param>
    internal static BankListResult ToBankListResult(this ResponseEnvelope envelope)
    {
        var banks = new List<MoolreBank>();

        if (envelope.Data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in envelope.Data.EnumerateArray())
            {
                // An entry that is not an object reads as blanks instead of throwing, so one bad entry cannot hide the others.
                banks.Add(new MoolreBank(item.ReadString("name") ?? string.Empty, item.ReadString("code") ?? string.Empty));
            }
        }

        return new BankListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, banks);
    }
}
