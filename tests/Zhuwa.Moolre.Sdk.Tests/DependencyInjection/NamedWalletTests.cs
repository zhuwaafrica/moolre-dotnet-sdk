using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Xunit;
using Zhuwa.Moolre.DependencyInjection;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests several wallets in one app. Moolre issues credentials per wallet, so each named wallet must send only its own keys and account
/// number, and an unknown name must fail clearly.
/// </summary>
public sealed class NamedWalletTests
{
    // Every request any wallet sends, with the headers and body it carried.
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<(string? User, string? Key, string Body, string Host)> Sent { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Sent.Add((
                request.Headers.TryGetValues("X-API-USER", out var user) ? user.Single() : null,
                request.Headers.TryGetValues("X-API-KEY", out var key) ? key.Single() : null,
                request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken),
                request.RequestUri!.Host));

            const string Body = """{"status":1,"code":"SW01","message":"Wallet Found","data":{"balance":1,"accountname":"W","activated":"1"}}""";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Body, Encoding.UTF8, "application/json") };
        }
    }

    // Replaces the network for every Moolre HttpClient (each wallet has its own), keeping everything else AddMoolre set up.
    private static ServiceProvider Build(RecordingHandler handler, Action<IServiceCollection> register)
    {
        var services = new ServiceCollection().AddLogging();
        register(services);
        services.ConfigureAll<HttpClientFactoryOptions>(o => o.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = handler));
        return services.BuildServiceProvider();
    }

    private static void TwoWallets(IServiceCollection services) => services
        .AddMoolre("collections", o => { o.ApiUser = "user-a"; o.PrivateKey = "key-a"; o.AccountNumber = "111"; })
        .AddMoolre("payouts", o => { o.ApiUser = "user-b"; o.PrivateKey = "key-b"; o.AccountNumber = "222"; o.Environment = MoolreEnvironment.Live; });

    [Fact]
    public async Task Create_ForEachWallet_SendsOnlyThatWalletsCredentialsAccountAndEnvironment()
    {
        var handler = new RecordingHandler();
        using var provider = Build(handler, TwoWallets);
        var factory = provider.GetRequiredService<MoolreClientFactory>();

        await factory.Create("collections").Accounts.GetStatusAsync(TestContext.Current.CancellationToken);
        await factory.Create("payouts").Accounts.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.Equal(("user-a", "key-a"), (handler.Sent[0].User, handler.Sent[0].Key));
        Assert.Contains("\"accountnumber\":\"111\"", handler.Sent[0].Body, StringComparison.Ordinal);
        Assert.Equal("sandbox.moolre.com", handler.Sent[0].Host);
        Assert.Equal(("user-b", "key-b"), (handler.Sent[1].User, handler.Sent[1].Key));
        Assert.Contains("\"accountnumber\":\"222\"", handler.Sent[1].Body, StringComparison.Ordinal);
        Assert.Equal("api.moolre.com", handler.Sent[1].Host);
    }

    [Fact]
    public void Create_TheSameNameTwice_ReturnsTheSameClient()
    {
        using var provider = Build(new RecordingHandler(), TwoWallets);
        var factory = provider.GetRequiredService<MoolreClientFactory>();

        Assert.Same(factory.Create("collections"), factory.Create("collections"));
        Assert.NotSame(factory.Create("collections"), factory.Create("payouts"));
    }

    [Fact]
    public void Create_ForANameThatWasNotRegistered_ThrowsAClearError()
    {
        using var provider = Build(new RecordingHandler(), TwoWallets);

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MoolreClientFactory>().Create("refunds"));

        Assert.Contains("refunds", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_ForAWalletWithInvalidSettings_ThrowsValidation()
    {
        using var provider = Build(new RecordingHandler(), s => s.AddMoolre("broken", o => o.Timeout = TimeSpan.Zero));

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<MoolreClientFactory>().Create("broken"));
    }

    [Fact]
    public void NamedWalletsOnly_DoNotRegisterADefaultClient()
    {
        using var provider = Build(new RecordingHandler(), TwoWallets);

        Assert.Null(provider.GetService<MoolreClient>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MoolreClientFactory>().Create(string.Empty));
    }

    [Fact]
    public async Task DefaultAndNamedWallets_CanBeUsedTogether()
    {
        var handler = new RecordingHandler();
        using var provider = Build(handler, s =>
        {
            s.AddMoolre(o => { o.ApiUser = "default-user"; o.AccountNumber = "000"; });
            TwoWallets(s);
        });

        await provider.GetRequiredService<MoolreClient>().Accounts.GetStatusAsync(TestContext.Current.CancellationToken);
        await provider.GetRequiredService<MoolreClientFactory>().Create("payouts").Accounts.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.Equal("default-user", handler.Sent[0].User);
        Assert.Equal("user-b", handler.Sent[1].User);
        Assert.Same(provider.GetRequiredService<MoolreClient>(), provider.GetRequiredService<MoolreClientFactory>().Create(string.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddMoolre_WithABlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddMoolre(name, _ => { }));
    }

    [Fact]
    public void AddMoolre_ForEachWallet_GivesItItsOwnNamedHttpClient()
    {
        using var provider = new ServiceCollection().AddLogging().AddMoolre("payouts", o => o.Environment = MoolreEnvironment.Live).BuildServiceProvider();

        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("Zhuwa.Moolre:payouts");

        Assert.Equal(new Uri("https://api.moolre.com"), client.BaseAddress);
    }
}
