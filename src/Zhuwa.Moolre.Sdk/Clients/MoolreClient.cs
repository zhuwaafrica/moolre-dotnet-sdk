using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Reference;

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

    /// <summary>Banks and other reference data (no credentials needed).</summary>
    public virtual MoolreReferenceData ReferenceData => throw new NotImplementedException();
}
