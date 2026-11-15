using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Reference;

namespace Zhuwa.Moolre;

// Joins the area clients behind the one MoolreClient that applications inject. Each area is created once by dependency
// injection and shared, because the areas hold no per-call state.
internal sealed class DefaultMoolreClient(MoolreAccounts accounts, MoolreReferenceData referenceData) : MoolreClient
{
    public override MoolreAccounts Accounts { get; } = accounts;

    public override MoolreReferenceData ReferenceData { get; } = referenceData;
}
