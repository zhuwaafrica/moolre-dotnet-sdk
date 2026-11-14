using Xunit;

namespace Zhuwa.Moolre;

public sealed class ReferenceDataClientTests
{
    [Fact]
    public async Task GetBanksAsync_ForTheDocumentedExample_MapsEveryBank()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("miscellaneous-data/response-200-success.json"));

        var result = await sdk.ReferenceData.GetBanksAsync("gha");

        Assert.Equal(3, result.Banks.Count);
        Assert.Equal("ABSA BANK GHANA LTD", result.Banks[0].Name);
        Assert.Equal("030100", result.Banks[0].Code);
        Assert.Equal("?country=gha&data=banks", sdk.Handler.Request!.RequestUri!.Query);
    }

    [Fact]
    public async Task GetBanksAsync_EscapesTheCountryCode()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("miscellaneous-data/response-200-success.json"));

        await sdk.ReferenceData.GetBanksAsync("g&a");

        Assert.Equal("?country=g%26a&data=banks", sdk.Handler.Request!.RequestUri!.Query);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetBanksAsync_WithABlankCountry_Throws(string country)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("miscellaneous-data/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.ReferenceData.GetBanksAsync(country));
    }

    [Fact]
    public async Task GetBanksAsync_WhenAnEntryIsNotAnObject_KeepsTheListAndDoesNotThrow()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"SD01","data":["oops",{"name":"A","code":"1"},null]}"""));

        var result = await sdk.ReferenceData.GetBanksAsync("gha");

        Assert.Equal(3, result.Banks.Count);
        Assert.Equal("A", result.Banks[1].Name);
    }

    [Fact]
    public async Task GetBanksAsync_WhenDataIsNotAnArray_ReturnsNoBanks()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"SD01","data":"none"}"""));

        var result = await sdk.ReferenceData.GetBanksAsync("gha");

        Assert.Empty(result.Banks);
    }
}
