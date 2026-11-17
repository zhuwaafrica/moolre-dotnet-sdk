using Xunit;
using Zhuwa.Moolre.Sales;

namespace Zhuwa.Moolre;

/// <summary>
/// Sales against the real sandbox. The wallet must be linked to Moolre's "Sell on Our Platform" service. Each sale uses a reference
/// unique to the run.
/// </summary>
public sealed class SalesSandboxTests
{
    [SandboxFact]
    public async Task ListBundlesAsync_ReturnsBundlesWithIdsAndPrices()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.Sales.ListBundlesAsync(TestContext.Current.CancellationToken);

        Assert.All(result.Mtn.Concat(result.Telecel).Concat(result.At), bundle =>
        {
            Assert.False(string.IsNullOrWhiteSpace(bundle.BundleId));
            Assert.True(bundle.Price > 0m);
        });
    }

    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task SendAirtimeAsync_ThenGetStatusAsync_FindsTheSaleByItsReference()
    {
        using var sandbox = new SandboxClient();
        var reference = SandboxSettings.NewReference("airtime");

        var sale = await sandbox.Moolre.Sales.SendAirtimeAsync(
            new SendAirtimeRequest { Network = SalesNetwork.Mtn, ReceiverPhoneNumber = SandboxSettings.PhoneNumber!, Amount = 1m, ExternalReference = reference },
            TestContext.Current.CancellationToken);
        var status = await sandbox.Moolre.Sales.GetStatusAsync(
            new GetSaleStatusRequest { IdKind = SaleIdKind.ExternalReference, Id = reference },
            TestContext.Current.CancellationToken);

        Assert.Equal(reference, sale.ExternalReference);
        Assert.NotEqual(SaleStatus.NotFound, status.Status);
    }
}
