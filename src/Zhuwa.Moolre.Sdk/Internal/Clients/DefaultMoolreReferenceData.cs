using System.Text.Json;
using Zhuwa.Moolre.Internal.Serialization;
using Zhuwa.Moolre.Internal.Transport;
using Zhuwa.Moolre.Reference;

namespace Zhuwa.Moolre.Internal.Clients;

internal sealed class DefaultMoolreReferenceData(MoolreTransport transport) : MoolreReferenceData
{
    public override async Task<BankListResult> GetBanksAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        // Reference data has no request object, so the one argument is checked here.
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var envelope = await transport.SendAsync(
            MoolreEndpoints.ReferenceData.GetBanks,
            writeFields: null,
            // GET call with no credentials. The country is escaped so a value like "g&a" cannot add a query parameter.
            query: $"country={Uri.EscapeDataString(countryCode)}&data=banks",
            externalReference: null,
            cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        // data is an array of { name, code }. Anything else gives an empty list, not an error.
        var banks = new List<MoolreBank>();
        if (envelope.Data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in envelope.Data.EnumerateArray())
            {
                banks.Add(new MoolreBank(item.ReadString("name") ?? string.Empty, item.ReadString("code") ?? string.Empty));
            }
        }

        return new BankListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, banks);
    }
}
