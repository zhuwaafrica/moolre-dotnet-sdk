using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.Transfers;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre.Core;

// Joins one wallet's area clients behind the MoolreClient that applications use. DefaultMoolreClientFactory builds one per wallet;
// the areas hold no per-call state, so each is created once and shared.
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
