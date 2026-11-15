using System.Net;
using Xunit;
using Zhuwa.Moolre.Payments;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests mobile money collection: the prompt, the OTP challenge, and the money-safety rules (sent exactly once, and an unknown
/// outcome hands back the reference so the customer is never charged twice).
/// </summary>
public sealed class MobileMoneyOtpTests
{
    private static RequestMobileMoneyRequest Payment(string reference = "ORDER-1", string? otpCode = null, bool? skipOtp = null) => new()
    {
        Channel = PaymentChannel.Mtn,
        Currency = Currency.Ghs,
        PayerPhoneNumber = "0241234567",
        Amount = 25.5m,
        ExternalReference = reference,
        OtpCode = otpCode,
        SkipOtp = skipOtp,
    };

    [Fact]
    public async Task RequestMobileMoneyAsync_WhenThePromptIsSent_ReturnsPromptSentWithTheRequestId()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"));

        var result = await sdk.Payments.RequestMobileMoneyAsync(Payment());

        Assert.Equal(MobileMoneyOutcome.PromptSent, result.Outcome);
        Assert.Equal("f25fc80e-791b-495b-8799-dcf87660457d", result.RequestId);
        Assert.Equal(
            """{"type":1,"channel":"13","currency":"GHS","payer":"0241234567","amount":"25.5","externalref":"ORDER-1","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WhenAnOtpIsRequired_ReturnsOtpRequiredWithoutARequestId()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-otp-required-otp-required.json"));

        var result = await sdk.Payments.RequestMobileMoneyAsync(Payment());

        Assert.Equal(MobileMoneyOutcome.OtpRequired, result.Outcome);
        Assert.Null(result.RequestId);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WithTheCustomersCode_SendsTheOtpCode()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"));

        await sdk.Payments.RequestMobileMoneyAsync(Payment(otpCode: "123456"));

        Assert.Contains("\"otpcode\":\"123456\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_ForAnUnknownSuccessCode_ReportsNoneNotPromptSent()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"NEW99","message":"Something new","data":"abc"}"""));

        var result = await sdk.Payments.RequestMobileMoneyAsync(Payment());

        Assert.Equal(MobileMoneyOutcome.None, result.Outcome);
        Assert.Null(result.RequestId);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_InTheSandbox_SendsSkipOtp()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"));

        await sdk.Payments.RequestMobileMoneyAsync(Payment(skipOtp: true));

        Assert.Contains("\"skipotp\":true", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WithSkipOtpInLive_ThrowsAndSendsNothing()
    {
        var options = TestTransport.SandboxOptions();
        options.Environment = MoolreEnvironment.Live;
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"), options);

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Payments.RequestMobileMoneyAsync(Payment(skipOtp: true)));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_ForADuplicateReference_ThrowsDuplicateReferenceException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-400-duplicate-reference.json", HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => sdk.Payments.RequestMobileMoneyAsync(Payment()));

        Assert.Equal(MoolreResponseCode.CollectionDuplicateReference, exception.Code);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_ForAnInvalidCurrency_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"TP01","message":"Currency invalid"}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Payments.RequestMobileMoneyAsync(Payment()));

        Assert.Equal(MoolreResponseCode.CurrencyInvalid, exception.Code);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WhenItTimesOut_ThrowsUncertainWithTheReferenceAndSendsOnce()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering(), options);

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Payments.RequestMobileMoneyAsync(Payment("ORDER-77")));

        Assert.Equal("ORDER-77", exception.ExternalReference);
        Assert.Equal("Payments.RequestMobileMoneyAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WhenTheConnectionFails_ThrowsUncertainAndSendsOnce()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("connection reset")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Payments.RequestMobileMoneyAsync(Payment("ORDER-78")));

        Assert.Equal("ORDER-78", exception.ExternalReference);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Theory]
    [InlineData(PaymentChannel.Mtn, "13")]
    [InlineData(PaymentChannel.Telecel, "6")]
    [InlineData(PaymentChannel.At, "7")]
    public async Task RequestMobileMoneyAsync_SendsMoolresCollectionChannelCode(PaymentChannel channel, string expected)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"));
        var request = new RequestMobileMoneyRequest
        {
            Channel = channel,
            Currency = Currency.Ghs,
            PayerPhoneNumber = "0241234567",
            Amount = 1m,
            ExternalReference = "C-1",
        };

        await sdk.Payments.RequestMobileMoneyAsync(request);

        Assert.Contains($"\"channel\":\"{expected}\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestMobileMoneyAsync_WithAnInvalidAmountOrNoChannel_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("mobile-money-collection/response-200-payment-request-success.json"));
        var zeroAmount = new RequestMobileMoneyRequest { Channel = PaymentChannel.Mtn, Currency = Currency.Ghs, PayerPhoneNumber = "0", Amount = 0m, ExternalReference = "X" };
        var noChannel = new RequestMobileMoneyRequest { Channel = PaymentChannel.None, Currency = Currency.Ghs, PayerPhoneNumber = "0", Amount = 1m, ExternalReference = "X" };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Payments.RequestMobileMoneyAsync(zeroAmount));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Payments.RequestMobileMoneyAsync(noChannel));

        Assert.Equal(0, sdk.Handler.CallCount);
    }
}
