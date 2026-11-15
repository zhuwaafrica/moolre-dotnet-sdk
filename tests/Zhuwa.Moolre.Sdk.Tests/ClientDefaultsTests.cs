using Xunit;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests the public base classes as an app's own test doubles see them: the shorter overloads forward to the full one, and anything not
/// overridden says so clearly instead of returning nothing.
/// </summary>
public sealed class ClientDefaultsTests
{
    private sealed class BareClient : MoolreClient;

    private sealed class RecordingAccounts : MoolreAccounts
    {
        public object? LastRequest { get; private set; }

        public override Task<AccountStatusResult> GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<AccountStatusResult>(null!);
        }

        public override Task<AccountTransactionsResult> ListTransactionsAsync(ListAccountTransactionsRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<AccountTransactionsResult>(null!);
        }
    }

    private sealed class RecordingPayments : MoolrePayments
    {
        public ListBankAccountsRequest? LastRequest { get; private set; }

        public override Task<BankAccountListResult> ListBankAccountsAsync(ListBankAccountsRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<BankAccountListResult>(null!);
        }
    }

    private sealed class RecordingSales : MoolreSales
    {
        public ListBundlesRequest? LastRequest { get; private set; }

        public override Task<BundleCatalogResult> ListBundlesAsync(ListBundlesRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<BundleCatalogResult>(null!);
        }
    }

    private sealed class RecordingSms : MoolreSms
    {
        public int Calls { get; private set; }

        public override Task<SenderIdListResult> ListSenderIdsAsync(CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<SenderIdListResult>(null!);
        }

        public override Task<SmsBalanceResult> GetBalanceAsync(CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<SmsBalanceResult>(null!);
        }
    }

    private sealed class RecordingWhatsApp : MoolreWhatsApp
    {
        public GetWhatsAppTemplatesRequest? LastRequest { get; private set; }

        public override Task<WhatsAppTemplatesPage> GetTemplatePageAsync(GetWhatsAppTemplatesRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<WhatsAppTemplatesPage>(null!);
        }
    }

    [Fact]
    public void MoolreClient_WithoutOverrides_ThrowsNotImplementedForEveryArea()
    {
        var client = new BareClient();

        Assert.Throws<NotImplementedException>(() => client.Accounts);
        Assert.Throws<NotImplementedException>(() => client.Payments);
        Assert.Throws<NotImplementedException>(() => client.Sales);
        Assert.Throws<NotImplementedException>(() => client.Sms);
        Assert.Throws<NotImplementedException>(() => client.Transfers);
        Assert.Throws<NotImplementedException>(() => client.WhatsApp);
        Assert.Throws<NotImplementedException>(() => client.ReferenceData);
    }

    [Fact]
    public async Task ShortOverloads_ForwardToTheFullOverloadWithAnEmptyRequest()
    {
        var accounts = new RecordingAccounts();
        var payments = new RecordingPayments();
        var sales = new RecordingSales();
        var sms = new RecordingSms();
        var whatsApp = new RecordingWhatsApp();

        await accounts.GetStatusAsync();
        Assert.IsType<GetAccountStatusRequest>(accounts.LastRequest);
        await accounts.ListTransactionsAsync(TestContext.Current.CancellationToken);
        Assert.IsType<ListAccountTransactionsRequest>(accounts.LastRequest);
        await payments.ListBankAccountsAsync();
        Assert.Null(payments.LastRequest!.AccountNumber);
        await sales.ListBundlesAsync();
        Assert.Null(sales.LastRequest!.Network);
        await sms.ListSenderIdsAsync();
        await sms.GetBalanceAsync();
        Assert.Equal(2, sms.Calls);
        await whatsApp.GetTemplatePageAsync();
        Assert.Null(whatsApp.LastRequest!.Cursor);
    }

    [Fact]
    public void MoolreClientFactory_WithoutAnOverride_ThrowsNotImplemented()
    {
        var factory = new BareFactory();

        Assert.Throws<NotImplementedException>(() => factory.Create("any"));
    }

    private sealed class BareFactory : MoolreClientFactory;

    private sealed class BareAccounts : MoolreAccounts;

    private sealed class BarePayments : MoolrePayments;

    private sealed class BareSales : MoolreSales;

    private sealed class BareSms : MoolreSms;

    private sealed class BareTransfers : Transfers.MoolreTransfers;

    private sealed class BareWhatsApp : MoolreWhatsApp;

    private sealed class BareReferenceData : Reference.MoolreReferenceData;

    [Fact]
    public async Task EveryAreaClient_WithoutOverrides_ThrowsNotImplementedForEveryCall()
    {
        Func<Task>[] calls =
        [
            () => new BareAccounts().GetStatusAsync(new GetAccountStatusRequest()),
            () => new BareAccounts().CreateSandboxAccountAsync(null!),
            () => new BareAccounts().UpdateAsync(null!),
            () => new BareAccounts().ListTransactionsAsync(new ListAccountTransactionsRequest()),
            () => new BarePayments().RequestMobileMoneyAsync(null!),
            () => new BarePayments().GetStatusAsync(null!),
            () => new BarePayments().CreatePaymentLinkAsync(null!),
            () => new BarePayments().CreatePaymentIdAsync(null!),
            () => new BarePayments().CreateBankAccountAsync(null!),
            () => new BarePayments().ListBankAccountsAsync(new ListBankAccountsRequest()),
            () => new BarePayments().GetPaymentAccountAsync(null!),
            () => new BareSales().SendAirtimeAsync(null!),
            () => new BareSales().SendDataAsync(null!),
            () => new BareSales().ListBundlesAsync(new ListBundlesRequest()),
            () => new BareSales().GetStatusAsync(null!),
            () => new BareSms().SendAsync(null!),
            () => new BareSms().SendSingleAsync(null!),
            () => new BareSms().GetStatusAsync(null!),
            () => new BareSms().CreateSenderIdsAsync(null!),
            () => new BareSms().GetSenderIdStatusAsync(null!),
            () => new BareSms().ListSenderIdsAsync(CancellationToken.None),
            () => new BareSms().ApproveSenderIdsAsync(null!),
            () => new BareSms().GetBalanceAsync(CancellationToken.None),
            () => new BareTransfers().ValidateNameAsync(null!),
            () => new BareTransfers().SendAsync(null!),
            () => new BareTransfers().GetStatusAsync(null!),
            () => new BareTransfers().SendInternalAsync(null!),
            () => new BareWhatsApp().GetTemplatePageAsync(new GetWhatsAppTemplatesRequest()),
            () => new BareWhatsApp().SendAsync(null!),
            () => new BareWhatsApp().GetStatusAsync(null!),
            () => new BareReferenceData().GetBanksAsync("gha"),
        ];

        foreach (var call in calls)
        {
            await Assert.ThrowsAsync<NotImplementedException>(call);
        }
    }
}
