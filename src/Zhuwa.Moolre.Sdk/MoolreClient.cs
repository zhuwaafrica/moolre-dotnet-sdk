using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>The entry point to the Moolre API. Inject this; each property is one area of the API.</summary>
public abstract class MoolreClient
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreClient()
    {
    }

    /// <summary>Wallet details and account transactions.</summary>
    public virtual MoolreAccounts Accounts => throw new NotImplementedException();

    /// <summary>Payments: mobile money prompts, payment links, Payment IDs, bank collections and payment status.</summary>
    public virtual MoolrePayments Payments => throw new NotImplementedException();

    /// <summary>Transfers to mobile money, banks and other Moolre wallets, name checks and transfer status.</summary>
    public virtual MoolreTransfers Transfers => throw new NotImplementedException();

    /// <summary>Banks and other reference data (no credentials needed).</summary>
    public virtual MoolreReferenceData ReferenceData => throw new NotImplementedException();
}
