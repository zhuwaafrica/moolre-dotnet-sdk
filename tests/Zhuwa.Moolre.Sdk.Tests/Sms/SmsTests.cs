using System.Net;
using System.Text.Json.Nodes;
using Xunit;
using Zhuwa.Moolre.Sms;

namespace Zhuwa.Moolre;

/// <summary>Tests sending SMS, delivery status and the SMS balance through the stub handler.</summary>
public sealed class SmsTests
{
    private static SendSmsRequest Bulk(string senderId = "SmartSMS", SmsDeliveryNetwork network = SmsDeliveryNetwork.Default)
    {
        var request = new SendSmsRequest { SenderId = senderId, Network = network };
        request.Messages.Add(new SmsMessage { Recipient = "233241234567", Text = "Hello", Reference = "uuid--001" });
        return request;
    }

    private static void AssertBody(string expected, string? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual!)), actual);

    // ---- Send (POST)

    [Fact]
    public async Task SendAsync_ForTheDocumentedExample_SendsTheMessagesWithTheSmsKeyOnly()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"));

        var result = await sdk.Sms.SendAsync(Bulk());

        Assert.Equal(MoolreResponseCode.SmsSent, result.Code);
        AssertBody(
            """{"type":1,"senderid":"SmartSMS","messages":[{"recipient":"233241234567","message":"Hello","ref":"uuid--001"}]}""",
            sdk.Handler.RequestBody);
        Assert.Equal("sms-vas-key", sdk.Handler.Request!.Headers.GetValues("X-API-VASKEY").Single());
        Assert.False(sdk.Handler.Request.Headers.Contains("X-API-USER"));
        Assert.False(sdk.Handler.Request.Headers.Contains("X-API-KEY"));
    }

    [Theory]
    [InlineData(SmsDeliveryNetwork.Telecel, "TELECEL")]
    [InlineData(SmsDeliveryNetwork.Mtn, "MTN")]
    public async Task SendAsync_WithADeliveryNetwork_SendsIt(SmsDeliveryNetwork network, string expected)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"));

        await sdk.Sms.SendAsync(Bulk(network: network));

        Assert.Contains($"\"network\":\"{expected}\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_WithTheDefaultNetwork_LeavesTheNetworkOut()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"));

        await sdk.Sms.SendAsync(Bulk());

        Assert.DoesNotContain("network", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_WithAnUnapprovedSenderId_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-400-unapproved-sender-id.json", HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Sms.SendAsync(Bulk()));

        Assert.Equal(MoolreResponseCode.SenderIdNotApproved, exception.Code);
        Assert.Equal("senderid", exception.Detail);
    }

    [Fact]
    public async Task SendAsync_WithABadKey_ThrowsAuthenticationException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-401-authentication-error.json", HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<MoolreAuthenticationException>(() => sdk.Sms.SendAsync(Bulk()));
    }

    [Fact]
    public async Task SendAsync_WhenItTimesOut_ThrowsUncertainAndSendsOnce()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering(), options);

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Sms.SendAsync(Bulk()));

        Assert.Equal("Sms.SendAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithoutTheSmsKey_FailsBeforeAnyRequest()
    {
        var options = TestTransport.SandboxOptions();
        options.SmsVasKey = null;
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sdk.Sms.SendAsync(Bulk()));

        Assert.Contains("SmsVasKey", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithALongSenderIdOrNoMessages_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Sms.SendAsync(Bulk("TwelveChars1")));
        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Sms.SendAsync(new SendSmsRequest { SenderId = "SmartSMS" }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Send (GET)

    [Fact]
    public async Task SendSingleAsync_SendsEverythingInTheQueryStringIncludingTheType()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms-get/response-200-success.json"));

        await sdk.Sms.SendSingleAsync(new SendSingleSmsRequest { SenderId = "SmartSMS", Recipient = "233241234567", Text = "Hi & bye" });

        Assert.Equal(HttpMethod.Get, sdk.Handler.Request!.Method);
        Assert.Equal("?type=1&senderid=SmartSMS&recipient=233241234567&message=Hi%20%26%20bye", sdk.Handler.Request.RequestUri!.Query);
        Assert.Null(sdk.Handler.RequestBody);
        Assert.DoesNotContain("sms-vas-key", sdk.Handler.Request.RequestUri.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendSingleAsync_WithTextOver160Characters_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms-get/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sdk.Sms.SendSingleAsync(new SendSingleSmsRequest { SenderId = "SmartSMS", Recipient = "1", Text = new string('a', 161) }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Status and balance

    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_ReturnsOneStatusPerReference()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("sms-status/response-200-success.json"));
        var request = new GetSmsStatusRequest();
        request.References.Add("0338954001737166274");
        request.References.Add("uuid--001");

        var result = await sdk.Sms.GetStatusAsync(request);

        Assert.Equal(2, result.Statuses.Count);
        Assert.Equal("uuid--001", result.Statuses[1].Reference);
        Assert.Equal(2, result.Statuses[1].Status);
        AssertBody("""{"type":5,"ref":["0338954001737166274","uuid--001"]}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task GetStatusAsync_WithNoReferences_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("sms-status/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Sms.GetStatusAsync(new GetSmsStatusRequest()));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task GetBalanceAsync_ForTheDocumentedExample_ReturnsTheBalance()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("sms-account-status/response-200-success.json"));

        var result = await sdk.Sms.GetBalanceAsync();

        Assert.Equal(857m, result.Balance);
        AssertBody("""{"type":2}""", sdk.Handler.RequestBody);
    }
}
