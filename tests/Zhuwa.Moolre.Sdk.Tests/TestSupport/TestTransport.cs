using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Internal.Transport;
using Zhuwa.Moolre.Reference;

namespace Zhuwa.Moolre;

/// <summary>Builds the SDK's internals around a stub handler, so tests run without a network.</summary>
internal sealed class TestTransport
{
    private TestTransport(StubHttpHandler handler, MoolreOptions options)
    {
        Handler = handler;
        Transport = new MoolreTransport(new StubHttpClientFactory(handler, options), new FixedOptionsMonitor(options), NullLogger<MoolreTransport>.Instance);
    }

    public StubHttpHandler Handler { get; }

    public MoolreTransport Transport { get; }

    public MoolreAccounts Accounts => new DefaultMoolreAccounts(Transport);

    public MoolreReferenceData ReferenceData => new DefaultMoolreReferenceData(Transport);

    public static MoolreOptions SandboxOptions() => new()
    {
        Environment = MoolreEnvironment.Sandbox,
        ApiUser = "api-user",
        PrivateKey = "private-key",
        PublicKey = "public-key",
        AccountNumber = "10772306069982",
    };

    public static TestTransport Create(StubHttpHandler handler, MoolreOptions? options = null) => new(handler, options ?? SandboxOptions());

    private sealed class FixedOptionsMonitor(MoolreOptions options) : IOptionsMonitor<MoolreOptions>
    {
        public MoolreOptions CurrentValue { get; } = options;

        public MoolreOptions Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<MoolreOptions, string?> listener) => null;
    }

    private sealed class StubHttpClientFactory(StubHttpHandler handler, MoolreOptions options) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var client = new HttpClient(handler, disposeHandler: false);
            MoolreHttpClientConfiguration.Configure(client, options);
            return client;
        }
    }
}
