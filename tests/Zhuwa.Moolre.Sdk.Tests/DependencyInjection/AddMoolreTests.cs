using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zhuwa.Moolre.DependencyInjection;

namespace Zhuwa.Moolre;

public sealed class AddMoolreTests
{
    [Fact]
    public void AddMoolre_Always_MakesTheClientAvailableWithItsAreas()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options => options.ApiUser = "api-user")
            .BuildServiceProvider();

        var client = provider.GetRequiredService<MoolreClient>();

        Assert.NotNull(client.Accounts);
        Assert.NotNull(client.ReferenceData);
        Assert.Same(client, provider.GetRequiredService<MoolreClient>());
    }

    [Fact]
    public void AddMoolre_WithAnInvalidTimeout_FailsWhenTheOptionsAreRead()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options => options.Timeout = TimeSpan.Zero)
            .BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreOptions>>().Value);

        Assert.Contains("Timeout", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddMoolre_WithAnHttpBaseAddress_FailsValidation()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options => options.BaseAddress = new Uri("http://example.test"))
            .BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreOptions>>().Value);
    }

    [Fact]
    public void AddMoolre_WithANullCallback_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddMoolre(null!));
    }

    [Fact]
    public void Environment_ByDefault_IsSandbox()
    {
        using var provider = new ServiceCollection().AddLogging().AddMoolre(_ => { }).BuildServiceProvider();

        Assert.Equal(MoolreEnvironment.Sandbox, provider.GetRequiredService<IOptions<MoolreOptions>>().Value.Environment);
    }

    [Fact]
    public void AddMoolre_WithAKeyContainingALineBreak_FailsValidation()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options => options.PrivateKey = "key\r\nX-Evil: 1")
            .BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreOptions>>().Value);

        Assert.Contains("PrivateKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddMoolre_Always_DoesNotFollowRedirectsBecauseTheCredentialsAreCustomHeaders()
    {
        using var provider = new ServiceCollection().AddLogging().AddMoolre(_ => { }).BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("Zhuwa.Moolre");
        while (handler is DelegatingHandler delegating)
        {
            handler = delegating.InnerHandler!;
        }

        var primary = Assert.IsType<SocketsHttpHandler>(handler);
        Assert.False(primary.AllowAutoRedirect);
    }

    [Fact]
    public void AddMoolre_Always_ConfiguresTheHttpClientForTheChosenEnvironment()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options => options.Environment = MoolreEnvironment.Live)
            .BuildServiceProvider();

        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("Zhuwa.Moolre");

        Assert.Equal(new Uri("https://api.moolre.com"), client.BaseAddress);
        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }
}
