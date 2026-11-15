using System.Net;
using Xunit;
using Zhuwa.Moolre.Payments;

namespace Zhuwa.Moolre;

/// <summary>Tests payment links: the body Moolre expects (including its text flags and minute expiry) and how answers are read.</summary>
public sealed class PaymentLinkTests
{
    private static CreatePaymentLinkRequest Link(TimeSpan? expiresAfter = null) => new()
    {
        Amount = 100m,
        Email = "shop@example.com",
        ExternalReference = "LINK-1",
        Currency = Currency.Ghs,
        ExpiresAfter = expiresAfter,
    };

    [Fact]
    public async Task CreatePaymentLinkAsync_ForTheDocumentedExample_ReturnsTheLink()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("generate-payment-link/response-200-success.json"));

        var result = await sdk.Payments.CreatePaymentLinkAsync(Link());

        Assert.Equal(new Uri("https://pos.moolre.com/RZWs1yB6amGjNoiEQvlHPS5uqgp3Jc"), result.AuthorizationUrl);
        Assert.Equal("uuid-1234as2", result.Reference);
        Assert.Equal(
            """{"type":1,"amount":"100","email":"shop@example.com","externalref":"LINK-1","reusable":"0","currency":"GHS","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_WithEveryOptionalField_WritesThemAsMoolreExpects()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("generate-payment-link/response-200-success.json"));
        var request = new CreatePaymentLinkRequest
        {
            Amount = 100m,
            Email = "shop@example.com",
            ExternalReference = "LINK-2",
            Currency = Currency.Ghs,
            CallbackUrl = new Uri("https://shop.example.com/moolre/callback"),
            RedirectUrl = new Uri("https://shop.example.com/thanks"),
            IsReusable = true,
            ExpiresAfter = TimeSpan.FromMinutes(30),
        };
        request.Metadata["orderId"] = "1001";

        await sdk.Payments.CreatePaymentLinkAsync(request);

        Assert.Equal(
            """{"type":1,"amount":"100","email":"shop@example.com","externalref":"LINK-2","callback":"https://shop.example.com/moolre/callback","redirect":"https://shop.example.com/thanks","reusable":"1","expiration_time":30,"currency":"GHS","accountnumber":"10772306069982","metadata":{"orderId":"1001"}}""",
            sdk.Handler.RequestBody);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.5)]
    [InlineData(0)]
    public async Task CreatePaymentLinkAsync_WithAnExpiryThatIsNotWholeMinutes_ThrowsAndSendsNothing(double minutes)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("generate-payment-link/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Payments.CreatePaymentLinkAsync(Link(TimeSpan.FromMinutes(minutes))));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_ForADuplicateReference_ThrowsDuplicateReferenceException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("generate-payment-link/response-400-duplicate-transaction.json", HttpStatusCode.BadRequest));

        await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => sdk.Payments.CreatePaymentLinkAsync(Link()));
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_WhenMoolreSendsNoLink_ThrowsResponseException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"POS09","message":"ok","data":{"reference":"r"}}"""));

        await Assert.ThrowsAsync<MoolreResponseException>(() => sdk.Payments.CreatePaymentLinkAsync(Link()));
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_ForAnUnsupportedChannel_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"TP09","message":"This channel does not support API topup."}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Payments.CreatePaymentLinkAsync(Link()));

        Assert.Equal(MoolreResponseCode.ChannelNotSupported, exception.Code);
    }
}
