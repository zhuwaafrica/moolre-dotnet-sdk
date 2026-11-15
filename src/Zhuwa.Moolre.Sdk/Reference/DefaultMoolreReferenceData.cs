using Zhuwa.Moolre.Internal.Transport;

namespace Zhuwa.Moolre.Reference;

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

        // The mapper reads the answer; this method only decides what to send and when a failure is a failure.
        return envelope.ToBankListResult();
    }
}
