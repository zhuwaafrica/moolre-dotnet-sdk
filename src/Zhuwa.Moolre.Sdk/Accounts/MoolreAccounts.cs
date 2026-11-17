namespace Zhuwa.Moolre.Accounts;

/// <summary>Wallets: create a sandbox wallet, update a wallet, and read its balance, settings and transactions.</summary>
public abstract class MoolreAccounts
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreAccounts()
    {
    }

    /// <summary>Gets the balance and settings of a wallet.</summary>
    /// <param name="request">Which wallet; the default wallet from <see cref="MoolreOptions.AccountNumber"/> when its account number is omitted.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The wallet's balance, name, activation state and callback URL.</returns>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options.</exception>
    /// <exception cref="MoolreAuthenticationException">Moolre rejected the credentials or the wallet.</exception>
    public virtual Task<AccountStatusResult> GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the balance and settings of the default wallet from <see cref="MoolreOptions.AccountNumber"/>.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The wallet's balance, name, activation state and callback URL.</returns>
    public virtual Task<AccountStatusResult> GetStatusAsync(CancellationToken cancellationToken) =>
        GetStatusAsync(new GetAccountStatusRequest(), cancellationToken);

    /// <summary>Gets the balance and settings of the default wallet from <see cref="MoolreOptions.AccountNumber"/>.</summary>
    /// <returns>The wallet's balance, name, activation state and callback URL.</returns>
    public virtual Task<AccountStatusResult> GetStatusAsync() =>
        GetStatusAsync(new GetAccountStatusRequest(), CancellationToken.None);

    /// <summary>
    /// Creates a wallet in the sandbox, for testing. Moolre only offers this in the sandbox; live wallets are created in the Moolre app.
    /// Not retried automatically.
    /// </summary>
    /// <param name="request">The wallet.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new wallet, including its account number and callback secret (sensitive: store it securely, never log it).</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid (the currency must be GHS).</exception>
    /// <exception cref="InvalidOperationException">The SDK is set to the live environment, or the API user is missing.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The wallet may or may not have been created.</exception>
    /// <exception cref="MoolreApiException">Moolre refused it (for example <c>APGW21</c>, the account name is already used).</exception>
    public virtual Task<AccountResult> CreateSandboxAccountAsync(CreateSandboxAccountRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Changes a wallet's name, callback URL, API access or settlement. Only the values you set are sent. Not retried automatically.
    /// </summary>
    /// <param name="request">The changes; the wallet is the default from <see cref="MoolreOptions.AccountNumber"/> when its account number is omitted.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The wallet after the change.</returns>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The change may or may not have been applied.</exception>
    /// <exception cref="MoolreApiException">Moolre refused it (for example <c>WCU01</c>, invalid account number).</exception>
    public virtual Task<AccountResult> UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists a wallet's transactions, optionally filtered by date and status. Safe to repeat. No transactions is an empty list.</summary>
    /// <param name="request">The wallet and filters; the default wallet when its account number is omitted.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The transactions.</returns>
    /// <exception cref="ArgumentException">The date range is reversed, the limit is not above zero, or the status is not a known value.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<AccountTransactionsResult> ListTransactionsAsync(ListAccountTransactionsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists the default wallet's transactions, with no filters. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The transactions.</returns>
    public virtual Task<AccountTransactionsResult> ListTransactionsAsync(CancellationToken cancellationToken) =>
        ListTransactionsAsync(new ListAccountTransactionsRequest(), cancellationToken);

    /// <summary>Lists the default wallet's transactions, with no filters. Safe to repeat.</summary>
    /// <returns>The transactions.</returns>
    public virtual Task<AccountTransactionsResult> ListTransactionsAsync() =>
        ListTransactionsAsync(new ListAccountTransactionsRequest(), CancellationToken.None);
}
