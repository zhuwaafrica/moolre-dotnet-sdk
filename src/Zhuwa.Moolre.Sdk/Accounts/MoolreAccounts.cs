namespace Zhuwa.Moolre.Accounts;

/// <summary>Wallet details and account transactions.</summary>
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
}
