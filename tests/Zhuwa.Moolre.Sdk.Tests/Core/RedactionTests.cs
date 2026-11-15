using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>
/// Secrets must never reach logs or exception messages: API keys, the API user and the callback secret. These tests capture
/// everything the SDK logs and every message it throws, then search them for the secret values.
/// </summary>
public sealed class RedactionTests
{
    private const string ApiUser = "SECRET-API-USER-1234";
    private const string PrivateKey = "SECRET-PRIVATE-KEY-5678";
    private const string PublicKey = "SECRET-PUBLIC-KEY-9012";

    [Fact]
    public async Task Transport_OnSuccessAndOnFailure_NeverLogsCredentials()
    {
        var logger = new CapturingLogger();
        var options = new MoolreOptions { ApiUser = ApiUser, PrivateKey = PrivateKey, PublicKey = PublicKey, AccountNumber = "1", Timeout = TimeSpan.FromMilliseconds(50) };

        await Send(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options, logger, MoolreEndpoints.Accounts.GetStatus);
        await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() =>
            Send(StubHttpHandler.NeverAnswering(), options, logger, MoolreEndpoints.Transfers.Send));
        await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() =>
            Send(StubHttpHandler.Throwing(new HttpRequestException("reset")), options, logger, MoolreEndpoints.Payments.RequestMobileMoney));

        Assert.NotEmpty(logger.Messages);
        Assert.All(logger.Messages, message =>
        {
            Assert.DoesNotContain(ApiUser, message, StringComparison.Ordinal);
            Assert.DoesNotContain(PrivateKey, message, StringComparison.Ordinal);
            Assert.DoesNotContain(PublicKey, message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task MissingCredentialErrors_NameTheSettingButNeverShowAValue()
    {
        var options = new MoolreOptions { Environment = MoolreEnvironment.Live, ApiUser = ApiUser, PublicKey = PublicKey, AccountNumber = "1" };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Send(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options, new CapturingLogger(), MoolreEndpoints.Accounts.GetStatus));

        Assert.Contains("PrivateKey", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(ApiUser, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(PublicKey, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WebhookParseErrors_NeverShowTheCallbackSecret()
    {
        const string CallbackSecret = "SECRET-CALLBACK-3456";

        var exception = Assert.Throws<MoolreResponseException>(() =>
            PaymentWebhook.Parse($$"""{"status":1,"code":"P01","data":"{{CallbackSecret}}"}"""));

        Assert.DoesNotContain(CallbackSecret, exception.ToString(), StringComparison.Ordinal);
    }

    private static Task<ResponseEnvelope> Send(StubHttpHandler handler, MoolreOptions options, ILogger<MoolreTransport> logger, MoolreEndpoint endpoint)
    {
        var transport = new MoolreTransport(new SingleClientFactory(handler, options), new FixedMonitor(options), logger);
        return transport.SendAsync(endpoint, writer => writer.WriteString("accountnumber", "1"), query: null, "REF-1", CancellationToken.None);
    }

    private sealed class CapturingLogger : ILogger<MoolreTransport>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            if (exception is not null)
            {
                Messages.Add(exception.ToString());
            }
        }
    }

    private sealed class FixedMonitor(MoolreOptions options) : IOptionsMonitor<MoolreOptions>
    {
        public MoolreOptions CurrentValue => options;

        public MoolreOptions Get(string? name) => options;

        public IDisposable? OnChange(Action<MoolreOptions, string?> listener) => null;
    }

    private sealed class SingleClientFactory(StubHttpHandler handler, MoolreOptions options) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var client = new HttpClient(handler, disposeHandler: false);
            MoolreHttpClientConfiguration.Configure(client, options);
            return client;
        }
    }
}
