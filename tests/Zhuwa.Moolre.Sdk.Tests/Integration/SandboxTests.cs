using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zhuwa.Moolre.DependencyInjection;

namespace Zhuwa.Moolre;

/// <summary>
/// Calls Moolre's real sandbox. They are skipped unless you opt in, so the normal test run needs no network or credentials.
/// Set <c>MOOLRE_SANDBOX_TESTS=1</c> to run them. The wallet test also needs <c>MOOLRE_SANDBOX_API_USER</c> and
/// <c>MOOLRE_SANDBOX_ACCOUNT_NUMBER</c> (never write these in a file).
/// </summary>
public sealed class SandboxTests
{
    private static string? Setting(string name) => Environment.GetEnvironmentVariable(name);

    private static ServiceProvider CreateProvider() =>
        new ServiceCollection()
            .AddLogging()
            .AddMoolre(options =>
            {
                options.Environment = MoolreEnvironment.Sandbox;
                options.ApiUser = Setting("MOOLRE_SANDBOX_API_USER");
                options.AccountNumber = Setting("MOOLRE_SANDBOX_ACCOUNT_NUMBER");
            })
            .BuildServiceProvider();

    [Fact]
    public async Task GetBanksAsync_AgainstTheSandbox_ReturnsBanksWithCodes()
    {
        Assert.SkipUnless(Setting("MOOLRE_SANDBOX_TESTS") == "1", "Set MOOLRE_SANDBOX_TESTS=1 to call the sandbox.");
        using var provider = CreateProvider();

        var result = await provider.GetRequiredService<MoolreClient>().ReferenceData.GetBanksAsync("gha", TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Banks);
        Assert.All(result.Banks, bank =>
        {
            Assert.False(string.IsNullOrWhiteSpace(bank.Name));
            Assert.False(string.IsNullOrWhiteSpace(bank.Code));
        });
    }

    [Fact]
    public async Task GetStatusAsync_AgainstTheSandbox_ReturnsTheWallet()
    {
        Assert.SkipUnless(
            Setting("MOOLRE_SANDBOX_TESTS") == "1" && Setting("MOOLRE_SANDBOX_API_USER") is not null && Setting("MOOLRE_SANDBOX_ACCOUNT_NUMBER") is not null,
            "Set MOOLRE_SANDBOX_TESTS, MOOLRE_SANDBOX_API_USER and MOOLRE_SANDBOX_ACCOUNT_NUMBER to call the sandbox.");
        using var provider = CreateProvider();

        var result = await provider.GetRequiredService<MoolreClient>().Accounts.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(result.AccountName));
    }
}
