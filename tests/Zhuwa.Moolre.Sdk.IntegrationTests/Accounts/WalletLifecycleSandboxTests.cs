using Xunit;
using Zhuwa.Moolre.Accounts;

namespace Zhuwa.Moolre;

/// <summary>
/// Creates a fresh sandbox wallet and works with it, so these tests need only the API user (no existing wallet).
/// </summary>
public sealed class WalletLifecycleSandboxTests
{
    [SandboxFact(SandboxNeeds.ApiUser)]
    public async Task CreateSandboxAccountAsync_ThenUpdateAsync_ThenListTransactionsAsync_WorkOnTheNewWallet()
    {
        using var sandbox = new SandboxClient();
        var name = "Zhuwa IT " + Guid.NewGuid().ToString("N")[..10];

        var created = await sandbox.Moolre.Accounts.CreateSandboxAccountAsync(
            new CreateSandboxAccountRequest
            {
                AccountName = name,
                CallbackUrl = new Uri("https://example.com/moolre/callback"),
                Settlement = new SettlementSettings(),
            },
            TestContext.Current.CancellationToken);

        var walletNumber = created.Account.AccountNumber;
        var updated = await sandbox.Moolre.Accounts.UpdateAsync(
            new UpdateAccountRequest { AccountNumber = walletNumber, CallbackUrl = new Uri("https://example.com/moolre/updated") },
            TestContext.Current.CancellationToken);
        var transactions = await sandbox.Moolre.Accounts.ListTransactionsAsync(
            new ListAccountTransactionsRequest { AccountNumber = walletNumber },
            TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(walletNumber));
        Assert.False(string.IsNullOrWhiteSpace(created.Account.Secret));
        Assert.Equal(walletNumber, updated.Account.AccountNumber);
        Assert.Empty(transactions.Transactions);
    }
}
