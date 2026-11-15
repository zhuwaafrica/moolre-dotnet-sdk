using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Accounts;

internal sealed class DefaultMoolreAccounts(MoolreTransport transport) : MoolreAccounts
{
    public override async Task<AccountStatusResult> GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The request wins; the default wallet from the options is the fallback. With neither, this throws before any network call.
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetAccountStatusRequest));

        // Read-only call: safe to repeat, no external reference. The "type" field comes from the endpoint table.
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Accounts.GetStatus,
            writer => writer.WriteString("accountnumber", accountNumber),
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);
        // Authentication failures were already thrown by the transport; any other status 0 is a real failure here.
        MoolreTransport.EnsureSuccess(envelope);

        // The mapper reads the answer; this method only decides what to send and when a failure is a failure.
        return envelope.ToAccountStatusResult();
    }
}
