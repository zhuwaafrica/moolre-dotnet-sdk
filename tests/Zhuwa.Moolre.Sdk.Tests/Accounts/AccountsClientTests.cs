using Xunit;
using Zhuwa.Moolre.Accounts;

namespace Zhuwa.Moolre;

public sealed class AccountsClientTests
{
    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_MapsTheWallet()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        var result = await sdk.Accounts.GetStatusAsync(new GetAccountStatusRequest { AccountNumber = "10772306069982" });

        Assert.Equal(MoolreResponseCode.WalletFound, result.Code);
        Assert.Equal(10.67m, result.Balance);
        Assert.Equal("Zagey", result.AccountName);
        Assert.True(result.IsActivated);
        Assert.Equal(new Uri("https://moolre.requestcatcher.com/test"), result.CallbackUrl);
        Assert.Equal("""{"type":1,"accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task GetStatusAsync_WithoutAnAccountNumber_UsesTheDefaultWallet()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        await sdk.Accounts.GetStatusAsync();

        Assert.Equal("""{"type":1,"accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task GetStatusAsync_WithNoAccountNumberAnywhere_FailsBeforeAnyRequest()
    {
        var options = TestTransport.SandboxOptions();
        options.AccountNumber = null;
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"), options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sdk.Accounts.GetStatusAsync());

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task GetStatusAsync_WhenApiAccessIsNotActivated_ThrowsAuthenticationException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-401-authentication-error.json"));

        await Assert.ThrowsAsync<MoolreAuthenticationException>(() => sdk.Accounts.GetStatusAsync());
    }

    [Fact]
    public async Task GetStatusAsync_WithANullRequest_Throws()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sdk.Accounts.GetStatusAsync(null!));
    }

    [Theory]
    [InlineData("""{"status":1,"code":"SW01","message":"Wallet Found","data":null}""")]
    [InlineData("""{"status":1,"code":"SW01","message":"Wallet Found","data":"unexpected"}""")]
    [InlineData("""{"status":1,"code":"SW01","message":"Wallet Found"}""")]
    [InlineData("""{"status":1,"code":"SW01","message":"Wallet Found","data":{"balance":"12.50","activated":1}}""")]
    public async Task GetStatusAsync_WhenDataIsMissingOrOddlyShaped_ReturnsDefaultsInsteadOfThrowing(string json)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(json));

        var result = await sdk.Accounts.GetStatusAsync();

        Assert.Equal(MoolreResponseCode.WalletFound, result.Code);
        Assert.Equal(string.Empty, result.AccountName);
    }

    [Fact]
    public async Task GetStatusAsync_WhenBalanceAndActivationAreStringsOrNumbers_ReadsBoth()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"SW01","data":{"balance":"12.50","activated":1}}"""));

        var result = await sdk.Accounts.GetStatusAsync();

        Assert.Equal(12.50m, result.Balance);
        Assert.True(result.IsActivated);
    }
}
