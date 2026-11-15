using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre.Core;

// Joins the area clients behind the one MoolreClient that applications inject. Each area is created once by dependency
// injection and shared, because the areas hold no per-call state.
internal sealed class DefaultMoolreClient(MoolreAccounts accounts, MoolrePayments payments, MoolreTransfers transfers, MoolreReferenceData referenceData) : MoolreClient
{
    public override MoolreAccounts Accounts { get; } = accounts;

    public override MoolrePayments Payments { get; } = payments;

    public override MoolreTransfers Transfers { get; } = transfers;

    public override MoolreReferenceData ReferenceData { get; } = referenceData;
}
