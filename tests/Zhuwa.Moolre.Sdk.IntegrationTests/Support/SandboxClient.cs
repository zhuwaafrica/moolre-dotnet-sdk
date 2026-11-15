using Microsoft.Extensions.DependencyInjection;
using Zhuwa.Moolre.DependencyInjection;

namespace Zhuwa.Moolre;

/// <summary>
/// Builds a real <see cref="MoolreClient"/> for the sandbox through <c>AddMoolre</c>, exactly as an application would, so the tests
/// cover dependency injection, options validation, the HttpClient setup and the transport end to end.
/// </summary>
internal sealed class SandboxClient : IDisposable
{
    private readonly ServiceProvider _provider;

    internal SandboxClient()
    {
        _provider = new ServiceCollection()
            .AddLogging()
            .AddMoolre(options =>
            {
                options.Environment = MoolreEnvironment.Sandbox;
                options.ApiUser = SandboxSettings.ApiUser;
                options.AccountNumber = SandboxSettings.AccountNumber;
                options.PrivateKey = SandboxSettings.PrivateKey;
                options.PublicKey = SandboxSettings.PublicKey;
            })
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Moolre = _provider.GetRequiredService<MoolreClient>();
    }

    internal MoolreClient Moolre { get; }

    public void Dispose() => _provider.Dispose();
}
