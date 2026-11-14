namespace Zhuwa.Moolre.Reference;

/// <summary>Moolre's lists of banks and other reference data. These calls need no credentials.</summary>
public abstract class MoolreReferenceData
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreReferenceData()
    {
    }

    /// <summary>Gets the banks Moolre supports in a country.</summary>
    /// <param name="countryCode">Moolre's country code, for example <c>gha</c>.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The banks. Use a bank's <see cref="MoolreBank.Code"/> as the bank code in requests.</returns>
    /// <exception cref="ArgumentException"><paramref name="countryCode"/> is null, empty or white space.</exception>
    public virtual Task<BankListResult> GetBanksAsync(string countryCode, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
