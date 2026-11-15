using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Reference;

/// <summary>Sends the reference data calls.</summary>
internal sealed class DefaultMoolreReferenceData(MoolreTransport transport) : MoolreReferenceData
{
    public override async Task<BankListResult> GetBanksAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        // Reference data has no request object, so the one argument is checked here.
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        // A GET call with no credentials; the fields become the query string, each value escaped.
        var query = MoolreJson.ToBody(new GetBanksRequest { CountryCode = countryCode }, MoolreJsonContext.Default.GetBanksRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.ReferenceData.GetBanks, query, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        // The mapper reads the answer; this method only decides what to send and when a failure is a failure.
        return envelope.ToBankListResult();
    }
}
