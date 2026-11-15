using System.Net;
using System.Text.Json;
using Xunit;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

public sealed class MoolreTransportTests
{
    private static Task<ResponseEnvelope> Send(
        TestTransport sdk,
        MoolreEndpoint endpoint,
        string? externalReference = null,
        CancellationToken cancellationToken = default) =>
        sdk.Transport.SendAsync(endpoint, writer => writer.WriteString("accountnumber", "1"), query: null, externalReference, cancellationToken);

    [Fact]
    public async Task SendAsync_ForAPostEndpoint_SendsTheTypeAndFieldsAsJson()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        await Send(sdk, MoolreEndpoints.Accounts.GetStatus);

        Assert.Equal(HttpMethod.Post, sdk.Handler.Request!.Method);
        Assert.Equal("/open/account/status", sdk.Handler.Request.RequestUri!.AbsolutePath);
        Assert.Equal("""{"type":1,"accountnumber":"1"}""", sdk.Handler.RequestBody);
        Assert.Equal("application/json", sdk.Handler.Request.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task SendAsync_ForAPrivateEndpoint_SendsTheUserAndPrivateKeyOnly()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        await Send(sdk, MoolreEndpoints.Accounts.GetStatus);

        var headers = sdk.Handler.Request!.Headers;
        Assert.Equal("api-user", headers.GetValues("X-API-USER").Single());
        Assert.Equal("private-key", headers.GetValues("X-API-KEY").Single());
        Assert.False(headers.Contains("X-API-PUBKEY"));
    }

    [Fact]
    public async Task SendAsync_ForAPublicEndpoint_SendsThePublicKey()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("payment-status/response-200-success.json"));

        await Send(sdk, MoolreEndpoints.Payments.GetStatus);

        Assert.Equal("public-key", sdk.Handler.Request!.Headers.GetValues("X-API-PUBKEY").Single());
        Assert.False(sdk.Handler.Request.Headers.Contains("X-API-KEY"));
    }

    [Fact]
    public async Task SendAsync_ForAnEndpointWithNoCredentials_SendsNoCredentialHeaders()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("miscellaneous-data/response-200-success.json"));

        await sdk.Transport.SendAsync(MoolreEndpoints.ReferenceData.GetBanks, null, "country=gha&data=banks", null, CancellationToken.None);

        Assert.False(sdk.Handler.Request!.Headers.Contains("X-API-USER"));
        Assert.Equal(HttpMethod.Get, sdk.Handler.Request.Method);
        Assert.Equal("?country=gha&data=banks", sdk.Handler.Request.RequestUri!.Query);
        Assert.Null(sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task SendAsync_InTheSandbox_DoesNotRequireKeys()
    {
        var options = new MoolreOptions { Environment = MoolreEnvironment.Sandbox, ApiUser = "api-user" };
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options);

        await Send(sdk, MoolreEndpoints.Accounts.GetStatus);

        Assert.False(sdk.Handler.Request!.Headers.Contains("X-API-KEY"));
    }

    [Fact]
    public async Task SendAsync_InLive_WithoutThePrivateKey_FailsBeforeAnyRequest()
    {
        var options = new MoolreOptions { Environment = MoolreEnvironment.Live, ApiUser = "api-user" };
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Contains("PrivateKey", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithoutAnApiUser_FailsBeforeAnyRequest()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), new MoolreOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_ForAMoneyEndpointThatTimesOut_ThrowsOutcomeUncertainWithTheReference()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering(), options);

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-1001"));

        Assert.Equal("T-1001", exception.ExternalReference);
        Assert.Equal("Transfers.SendAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_ForAMoneyEndpointWithANetworkError_ThrowsOutcomeUncertainAndDoesNotRetry()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("connection reset")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => Send(sdk, MoolreEndpoints.Payments.RequestMobileMoney, "ORDER-1"));

        Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_ForAMoneyEndpointThatAnswersWithHtml_ThrowsOutcomeUncertain()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("<html>Bad gateway</html>", HttpStatusCode.BadGateway));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-2"));

        Assert.IsType<MoolreResponseException>(exception.InnerException);
    }

    [Fact]
    public async Task SendAsync_ForAReadEndpointThatAnswersWithHtml_ThrowsResponseException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("<html>Bad gateway</html>", HttpStatusCode.BadGateway));

        var exception = await Assert.ThrowsAsync<MoolreResponseException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Equal(HttpStatusCode.BadGateway, exception.HttpStatusCode);
    }

    [Fact]
    public async Task SendAsync_ForAReadEndpointThatTimesOut_ThrowsTimeoutException()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering(), options);

        await Assert.ThrowsAsync<TimeoutException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));
    }

    [Fact]
    public async Task SendAsync_ForAMoneyEndpointCancelledAfterItStarted_ThrowsOutcomeUncertain()
    {
        using var cts = new CancellationTokenSource();
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering());
        var pending = Send(sdk, MoolreEndpoints.Transfers.Send, "T-3", cts.Token);

        await cts.CancelAsync();

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => pending);
        Assert.Equal("T-3", exception.ExternalReference);
    }

    [Fact]
    public async Task SendAsync_ForAReadEndpointCancelledAfterItStarted_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        var sdk = TestTransport.Create(StubHttpHandler.NeverAnswering());
        var pending = Send(sdk, MoolreEndpoints.Accounts.GetStatus, cancellationToken: cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Fact]
    public async Task SendAsync_WhenTheTokenIsAlreadyCancelled_ThrowsOperationCanceledAndSendsNothing()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-6", cts.Token));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithAKeyThatCannotBeAHeaderValue_FailsBeforeAnyRequest()
    {
        var options = TestTransport.SandboxOptions();
        options.PrivateKey = "bad\nkey";
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Contains("PrivateKey", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Theory]
    [InlineData("""{"status":1,"code":"X","message":"ok"}""", true)]
    [InlineData("""{"status":"1","code":"X","message":"ok"}""", true)]
    [InlineData("""{"status":0,"code":"X","message":"no"}""", false)]
    [InlineData("""{"status":"0","code":"X","message":"no"}""", false)]
    public async Task SendAsync_ReadsStatusAsANumberOrAString(string json, bool expected)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(json));

        var envelope = await Send(sdk, MoolreEndpoints.Accounts.GetStatus);

        Assert.Equal(expected, envelope.IsSuccess);
    }

    [Fact]
    public async Task SendAsync_ReadsAnArrayOfMessagesAndJoinsThem()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"WC02","message":["Wallet created","Welcome"],"data":null}"""));

        var envelope = await Send(sdk, MoolreEndpoints.Accounts.GetStatus);

        Assert.Equal(["Wallet created", "Welcome"], envelope.Messages);
        Assert.Equal("Wallet created Welcome", envelope.Message);
        Assert.Equal(JsonValueKind.Null, envelope.Data.ValueKind);
    }

    [Fact]
    public async Task SendAsync_ForAJsonValueThatIsNotAnEnvelope_ThrowsResponseException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("[1,2,3]"));

        await Assert.ThrowsAsync<MoolreResponseException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));
    }

    [Fact]
    public async Task SendAsync_ForAMoneyEndpointWhoseBodyReadFailsWithAnIoError_ThrowsOutcomeUncertain()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new IOException("connection reset while reading the body")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-4"));

        Assert.Equal("T-4", exception.ExternalReference);
    }

    [Fact]
    public async Task SendAsync_ForAReadEndpointWithANetworkError_LetsTheNetworkErrorThrough()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("connection reset")));

        await Assert.ThrowsAsync<HttpRequestException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    [InlineData("\"just a string\"")]
    public async Task SendAsync_ForAnEmptyOrNonObjectBody_ThrowsResponseException(string body)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(body));

        await Assert.ThrowsAsync<MoolreResponseException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));
    }

    [Fact]
    public async Task SendAsync_ForARedirectOnAMoneyEndpoint_IsTreatedAsUncertainNotFollowed()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(string.Empty, HttpStatusCode.Found));

        await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-5"));
    }
}
