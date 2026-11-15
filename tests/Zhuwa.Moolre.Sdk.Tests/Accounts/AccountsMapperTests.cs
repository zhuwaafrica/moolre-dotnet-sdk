using System.Net;
using Xunit;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>Tests the accounts mapper on its own: a fixture goes in, a result comes out, with no HTTP.</summary>
public sealed class AccountsMapperTests
{
    private static ResponseEnvelope Envelope(string fixture) =>
        ResponseEnvelope.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture)), HttpStatusCode.OK);

    [Fact]
    public void ToStatusResult_ForTheDocumentedExample_MapsEveryField()
    {
        var result = Envelope("account-status/response-200-success.json").ToAccountStatusResult();

        Assert.Equal(MoolreResponseCode.WalletFound, result.Code);
        Assert.Equal("Wallet Found", result.Message);
        Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.Equal(10.67m, result.Balance);
        Assert.Equal("Zagey", result.AccountName);
        Assert.True(result.IsActivated);
        Assert.Equal(new Uri("https://moolre.requestcatcher.com/test"), result.CallbackUrl);
    }

    [Fact]
    public void ToStatusResult_WhenDataIsAString_ReturnsDefaults()
    {
        var envelope = ResponseEnvelope.Parse("""{"status":1,"code":"SW01","data":"all"}"""u8, HttpStatusCode.OK);

        var result = envelope.ToAccountStatusResult();

        Assert.Equal(0m, result.Balance);
        Assert.Equal(string.Empty, result.AccountName);
        Assert.False(result.IsActivated);
        Assert.Null(result.CallbackUrl);
    }
}
