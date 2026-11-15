using Xunit;

namespace Zhuwa.Moolre;

/// <summary>Wallet calls against the real sandbox.</summary>
public sealed class AccountsSandboxTests
{
    [SandboxFact]
    public async Task GetStatusAsync_ForTheConfiguredWallet_ReturnsItsName()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.Accounts.GetStatusAsync(TestContext.Current.CancellationToken);

        Assert.Equal(MoolreResponseCode.WalletFound, result.Code);
        Assert.False(string.IsNullOrWhiteSpace(result.AccountName));
        Assert.True(result.Balance >= 0m);
    }
}
