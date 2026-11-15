using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Reference;

/// <summary>Turns Moolre's reference data responses into the public result types. Field names live on <see cref="BankWire"/>.</summary>
internal static class ReferenceDataMapper
{
    /// <summary>Maps a bank list response, whose <c>data</c> is an array of banks. Entries that are not objects are skipped.</summary>
    internal static BankListResult ToBankListResult(this ResponseEnvelope envelope)
    {
        var banks = MoolreJson.ReadDataList(envelope.Data, MoolreJsonContext.Default.BankWire)
            .Select(bank => new MoolreBank(bank.Name ?? string.Empty, bank.Code ?? string.Empty))
            .ToList();

        return new BankListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, banks);
    }
}
