using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Internal.Serialization;
using Zhuwa.Moolre.Internal.Transport;

namespace Zhuwa.Moolre.Internal.Clients;

internal sealed class DefaultMoolreAccounts(MoolreTransport transport) : MoolreAccounts
{
    public override async Task<AccountStatusResult> GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The request wins; the default wallet from the options is the fallback. With neither there is nothing to ask Moolre
        // about, so fail before any network call.
        var accountNumber = request.AccountNumber ?? transport.DefaultAccountNumber;
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new InvalidOperationException("Set GetAccountStatusRequest.AccountNumber or MoolreOptions.AccountNumber.");
        }

        // Read-only call: safe to repeat, no external reference. The "type" field comes from the endpoint table.
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Accounts.GetStatus,
            writer => writer.WriteString("accountnumber", accountNumber),
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);
        // Authentication failures were already thrown by the transport; any other status 0 is a real failure here.
        MoolreTransport.EnsureSuccess(envelope);

        // Each read tolerates a missing or oddly typed field (balance as a number or a string, activated as 1 or "1") and
        // returns a default instead of throwing, because Moolre's shapes vary.
        var data = envelope.Data;
        return new AccountStatusResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            data.ReadDecimal("balance"),
            data.ReadString("accountname") ?? string.Empty,
            data.ReadFlag("activated"),
            data.ReadUri("callback"));
    }
}
