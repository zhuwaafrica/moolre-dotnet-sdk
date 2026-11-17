using System.Net;
using Xunit;
using Zhuwa.Moolre.Sales;

namespace Zhuwa.Moolre;

public sealed class PendingSaleTests
{
    private static SaleResult CreateSale(MoolreResponseCode code) =>
        new(code, null, [], HttpStatusCode.OK, TransactionStatus.Pending, "233240000000", "Pending", "SALE-1", 1.00m);

    [Fact]
    public void IsInProgress_WhenCodeIsAsl11_IsTrue()
    {
        var sale = CreateSale(MoolreResponseCode.SaleInProgress);

        Assert.True(sale.IsInProgress);
    }

    [Fact]
    public void IsInProgress_WhenCodeIsAnythingElse_IsFalse()
    {
        var sale = CreateSale(MoolreResponseCode.SaleOutcomeUnknown);

        Assert.False(sale.IsInProgress);
    }

    [Fact]
    public void Result_Always_ExposesWhatMoolreSaid()
    {
        var sale = CreateSale(MoolreResponseCode.SaleInProgress);

        Assert.Equal("ASL11", sale.Code.Value);
        Assert.Equal(HttpStatusCode.OK, sale.HttpStatusCode);
        Assert.Equal("Pending", sale.TransactionId);
        Assert.Empty(sale.Messages);
    }

    [Fact]
    public void SaleStatus_Values_MatchMoolresDocumentedNumbers()
    {
        Assert.Equal(0, (int)SaleStatus.Pending);
        Assert.Equal(1, (int)SaleStatus.Sent);
        Assert.Equal(2, (int)SaleStatus.FailedAndRefunded);
        Assert.Equal(3, (int)SaleStatus.NotFound);
    }
}
