using System.Net;
using System.Text.Json.Nodes;
using Xunit;
using Zhuwa.Moolre.Sales;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests the sales calls through the stub handler: what is sent, how answers become results, and the money-safety rules (sent once,
/// and an unknown outcome hands back the reference).
/// </summary>
public sealed class SalesTests
{
    private static SendAirtimeRequest Airtime(string reference = "AIR-1", decimal amount = 1m) => new()
    {
        Network = SalesNetwork.Mtn,
        ReceiverPhoneNumber = "233240000000",
        Amount = amount,
        ExternalReference = reference,
    };

    private static SendDataRequest Data(string reference = "DATA-1") => new()
    {
        Network = SalesNetwork.Mtn,
        ReceiverPhoneNumber = "233240000000",
        BundleId = "RACT_Data_GHC_3_Others",
        ExternalReference = reference,
    };

    private static void AssertBody(string expected, string? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual!)), actual);

    // ---- Airtime

    [Fact]
    public async Task SendAirtimeAsync_ForTheDocumentedExample_ReturnsAnInProgressSale()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("airtime/response-200-in-progress-asl11.json"));

        var result = await sdk.Sales.SendAirtimeAsync(Airtime());

        Assert.True(result.IsInProgress);
        Assert.Equal(TransactionStatus.Pending, result.Status);
        Assert.Equal("Pending", result.TransactionId);
        Assert.Equal(1.00m, result.Amount);
        AssertBody(
            """{"type":1,"channel":"MTN","receiver":"233240000000","amount":"1","externalref":"AIR-1","currency":"GHS","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
        Assert.Equal("private-key", sdk.Handler.Request!.Headers.GetValues("X-API-KEY").Single());
    }

    [Fact]
    public async Task SendAirtimeAsync_WhenMoolreReportsAnUnknownError_ThrowsUncertainWithTheReference()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"ASL06","message":"Unknown error"}"""));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Sales.SendAirtimeAsync(Airtime("AIR-6")));

        Assert.Equal("AIR-6", exception.ExternalReference);
        Assert.Equal("Sales.SendAirtimeAsync", exception.Operation);
    }

    [Fact]
    public async Task SendAirtimeAsync_WhenItTimesOut_ThrowsUncertainAndSendsOnce()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering(), options);

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Sales.SendAirtimeAsync(Airtime("AIR-7")));

        Assert.Equal("AIR-7", exception.ExternalReference);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAirtimeAsync_ForADuplicateReference_ThrowsDuplicateReferenceException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"ASL08","message":"Reference already used"}"""));

        await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => sdk.Sales.SendAirtimeAsync(Airtime()));
    }

    [Fact]
    public async Task SendAirtimeAsync_WhenMoolreRefusesIt_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"ASL01","message":"Amount must be at least 1.00"}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Sales.SendAirtimeAsync(Airtime(amount: 0.5m)));

        Assert.Equal(MoolreResponseCode.SaleAmountInvalid, exception.Code);
    }

    [Fact]
    public async Task SendAirtimeAsync_WithoutANetworkOrWithAZeroAmount_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("airtime/response-200-in-progress-asl11.json"));
        var noNetwork = new SendAirtimeRequest { Network = SalesNetwork.None, ReceiverPhoneNumber = "1", Amount = 1m, ExternalReference = "X" };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Sales.SendAirtimeAsync(noNetwork));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Sales.SendAirtimeAsync(Airtime(amount: 0m)));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Data

    [Fact]
    public async Task SendDataAsync_ForTheDocumentedExample_SendsTheBundleAndNoAmount()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("data/response-200-in-progress-asl11.json"));

        var result = await sdk.Sales.SendDataAsync(Data());

        Assert.True(result.IsInProgress);
        Assert.Equal(3.00m, result.Amount);
        AssertBody(
            """{"type":1,"channel":"MTN","receiver":"233240000000","bundle":"RACT_Data_GHC_3_Others","externalref":"DATA-1","currency":"GHS","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Theory]
    [InlineData(SalesNetwork.Mtn, "MTN")]
    [InlineData(SalesNetwork.Telecel, "TELECEL")]
    [InlineData(SalesNetwork.At, "AT")]
    public async Task SendDataAsync_SendsTheNetworkName(SalesNetwork network, string expected)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("data/response-200-in-progress-asl11.json"));
        var request = new SendDataRequest { Network = network, ReceiverPhoneNumber = "1", BundleId = "B", ExternalReference = "R" };

        await sdk.Sales.SendDataAsync(request);

        Assert.Contains($"\"channel\":\"{expected}\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendDataAsync_WhenTheConnectionFails_ThrowsUncertainAndSendsOnce()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("reset")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Sales.SendDataAsync(Data("DATA-9")));

        Assert.Equal("DATA-9", exception.ExternalReference);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    // ---- Bundles

    [Fact]
    public async Task ListBundlesAsync_ForTheDocumentedExample_GroupsBundlesByNetwork()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("bundles/response-200-catalogue-example-data-bundles-asb02.json"));

        var result = await sdk.Sales.ListBundlesAsync();

        Assert.Equal(14, result.Mtn.Count);
        Assert.Equal(24, result.Telecel.Count);
        Assert.Empty(result.At);
        var first = result.Mtn[0];
        Assert.Equal("RACT_Data_GHC_0.5_Others", first.BundleId);
        Assert.Equal(0.50m, first.Price);
        Assert.Equal("20.73 MB", first.Label);
        Assert.Equal(20.73, first.Megabytes);
        Assert.Equal("No Expiry", first.Validity);
        Assert.Equal("Data Bundles", first.Category);
        AssertBody("""{"accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ListBundlesAsync_ForOneNetwork_SendsItsName()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("bundles/response-200-catalogue-example-data-bundles-asb02.json"));

        await sdk.Sales.ListBundlesAsync(new ListBundlesRequest { Network = SalesNetwork.Telecel });

        AssertBody("""{"network":"TELECEL","accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    // ---- Status

    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_ReturnsASentSaleWithItsDetails()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("sales-status/response-200-sent-ass01.json"));

        var result = await sdk.Sales.GetStatusAsync(new GetSaleStatusRequest { IdKind = SaleIdKind.ExternalReference, Id = "your-extid" });

        Assert.Equal(SaleStatus.Sent, result.Status);
        Assert.Equal("RACT_Data_GHC_3_Others", result.BundleId);
        Assert.Equal(3.00m, result.Amount);
        Assert.Equal(0.18m, result.Discount);
        Assert.Equal(2.82m, result.Charged);
        Assert.Null(result.RefundId);
        Assert.Equal(new DateTimeOffset(2026, 10, 16, 9, 0, 3, TimeSpan.Zero), result.UpdatedAt);
        AssertBody("""{"type":1,"idtype":1,"id":"your-extid","accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Theory]
    [InlineData("ASS02", 0, SaleStatus.FailedAndRefunded)]
    [InlineData("ASS03", 1, SaleStatus.Pending)]
    [InlineData("ASS04", 0, SaleStatus.NotFound)]
    public async Task GetStatusAsync_ForEachStatusCode_ReturnsAResultEvenWithStatusZero(string code, int status, SaleStatus expected)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning($$"""{"status":{{status}},"code":"{{code}}","message":"m","data":null}"""));

        var result = await sdk.Sales.GetStatusAsync(new GetSaleStatusRequest { IdKind = SaleIdKind.SaleReference, Id = "S" });

        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public async Task GetStatusAsync_ForAnUnknownError_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"ASS08","message":"Unknown error"}"""));

        await Assert.ThrowsAsync<MoolreApiException>(() =>
            sdk.Sales.GetStatusAsync(new GetSaleStatusRequest { IdKind = SaleIdKind.TransactionId, Id = "1" }));
    }

    [Fact]
    public async Task GetStatusAsync_ByTransactionIdOrSaleReference_SendsTheMatchingIdType()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("sales-status/response-200-sent-ass01.json"));

        await sdk.Sales.GetStatusAsync(new GetSaleStatusRequest { IdKind = SaleIdKind.SaleReference, Id = "SALE-REFERENCE" });

        Assert.Contains("\"idtype\":3", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }
}
