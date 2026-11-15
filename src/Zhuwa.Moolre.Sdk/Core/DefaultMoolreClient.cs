using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.Transfers;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre.Core;

// Joins the area clients behind the one MoolreClient that applications inject. Each area is created once by dependency
// injection and shared, because the areas hold no per-call state.
internal sealed class DefaultMoolreClient(MoolreAccounts accounts, MoolrePayments payments, MoolreSales sales, MoolreSms sms, MoolreTransfers transfers, MoolreWhatsApp whatsApp, MoolreReferenceData referenceData) : MoolreClient
{
    public override MoolreAccounts Accounts { get; } = accounts;

    public override MoolrePayments Payments { get; } = payments;

    public override MoolreSales Sales { get; } = sales;

    public override MoolreSms Sms { get; } = sms;

    public override MoolreTransfers Transfers { get; } = transfers;

    public override MoolreWhatsApp WhatsApp { get; } = whatsApp;

    public override MoolreReferenceData ReferenceData { get; } = referenceData;
}
