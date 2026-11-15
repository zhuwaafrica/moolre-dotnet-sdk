using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Accounts;

/// <summary>
/// Sends the wallet calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it
/// through the transport and decides which answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolreAccounts(MoolreTransport transport) : MoolreAccounts
{
    public override async Task<AccountStatusResult> GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // The request wins; the default wallet from the options is the fallback. With neither, this throws before any network call.
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetAccountStatusRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetAccountStatusRequest).SetAccountNumber(accountNumber);

        // Read-only call: safe to repeat, no external reference.
        var envelope = await transport.SendAsync(MoolreEndpoints.Accounts.GetStatus, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // Authentication failures were already thrown by the transport; any other status 0 is a real failure here.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToAccountStatusResult();
    }

    public override async Task<AccountResult> CreateSandboxAccountAsync(CreateSandboxAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request, transport);

        // Every settlement key is sent (empty when unset), which Moolre documents as "not configured".
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.CreateSandboxAccountRequest);

        // No external reference exists for this call; an unknown outcome is still reported, because the wallet may exist.
        var envelope = await transport.SendAsync(MoolreEndpoints.Accounts.CreateSandboxAccount, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // A duplicate name (APGW21) or unsupported currency (APGW06) arrives with status 0 and becomes MoolreApiException.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToAccountResult();
    }

    public override async Task<AccountResult> UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(UpdateAccountRequest));

        // Only the values the caller set are sent, so nothing else on the wallet changes ("api" in particular is sent only to change
        // API access, as Moolre asks). The required GHS currency is added by the request type.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.UpdateAccountRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Accounts.Update, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // An invalid account number (WCU01) or a wallet that is not found (APGW12) becomes MoolreApiException.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToAccountResult();
    }

    public override async Task<AccountTransactionsResult> ListTransactionsAsync(ListAccountTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ListAccountTransactionsRequest));

        // Dates are written in GMT and the limit as text, by the converters named on the request type.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.ListAccountTransactionsRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Accounts.ListTransactions, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // "No transactions" (ST09) is an answer, not a failure; its body is not documented, so it is accepted whatever its status.
        if (envelope.Code != MoolreResponseCode.TransactionsNotFound)
        {
            MoolreTransport.EnsureSuccess(envelope);
        }

        return envelope.ToAccountTransactionsResult();
    }

    // Checks a sandbox wallet request: name, an http or https callback URL, settlement, GHS, and that the SDK is set to the sandbox. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(CreateSandboxAccountRequest request, MoolreTransport transport)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AccountName);
        ArgumentNullException.ThrowIfNull(request.CallbackUrl);
        ArgumentNullException.ThrowIfNull(request.Settlement);

        // Moolre offers this endpoint only in the sandbox; refusing here avoids a confusing error from the live API.
        if (!transport.IsSandbox)
        {
            throw new InvalidOperationException("CreateSandboxAccountAsync only works in the sandbox. Create live wallets in the Moolre app.");
        }

        // Moolre documents GHS as the only currency for new wallets.
        if (request.Currency != Currency.Ghs)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Currency, "Sandbox wallets support only Currency.Ghs.");
        }

        RequestGuard.ThrowIfNotWebUrl(request.CallbackUrl);
    }

    // Checks a wallet update: an http or https callback URL when one is set. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(UpdateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestGuard.ThrowIfNotWebUrl(request.CallbackUrl);
    }

    // Checks a transaction list request: date range order, a positive limit and a known status. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(ListAccountTransactionsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.From is { } from && request.To is { } to && from > to)
        {
            throw new ArgumentException("From must not be later than To.", nameof(request));
        }

        if (request.Limit is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Limit, "The limit must be greater than zero.");
        }

        if (request.Status is TransactionStatus status)
        {
            _ = status.ToWire(nameof(request.Status));
        }
    }
}
