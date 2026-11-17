using Xunit;

namespace Zhuwa.Moolre;

/// <summary>Reference data against the real sandbox. Needs no credentials, only the opt-in switch.</summary>
public sealed class ReferenceDataSandboxTests
{
    [SandboxFact(SandboxNeeds.None)]
    public async Task GetBanksAsync_ReturnsBanksThatEachHaveANameAndCode()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.ReferenceData.GetBanksAsync("gha", TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Banks);
        Assert.All(result.Banks, bank =>
        {
            Assert.False(string.IsNullOrWhiteSpace(bank.Name));
            Assert.False(string.IsNullOrWhiteSpace(bank.Code));
        });
    }
}
