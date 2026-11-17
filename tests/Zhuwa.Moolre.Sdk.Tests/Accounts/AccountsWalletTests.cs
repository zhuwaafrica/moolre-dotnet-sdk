using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Zhuwa.Moolre.Accounts;

namespace Zhuwa.Moolre;

/// <summary>Tests creating, updating and listing a wallet through the stub handler.</summary>
public sealed class  AccountsWalletTests
{
    private static CreateSandboxAccountRequest NewWallet(SettlementSettings? settlement = null) => new()
    {
        AccountName = "My Business LTD",
        CallbackUrl = new Uri("https://example.com/moolre/callback"),
        Settlement = settlement ?? new SettlementSettings(),
    };

    // ---- Create (sandbox only)

    [Fact]
    public async Task CreateSandboxAccountAsync_ForTheDocumentedExample_ReturnsTheWalletAndItsSecret()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-account-sandbox/response-200-success.json"));

        var result = await sdk.Accounts.CreateSandboxAccountAsync(NewWallet());

        Assert.Equal(MoolreResponseCode.WalletCreated, result.Code);
        Assert.Equal("100000157291", result.Account.AccountNumber);
        Assert.Equal("My Business LTD", result.Account.AccountName);
        Assert.Equal("0757291", result.Account.PaymentId);
        Assert.True(result.Account.IsEnabled);
        Assert.False(result.Account.ApiEnabled);
        Assert.Null(result.Account.CallbackUrl);
        Assert.Null(result.Account.Settlement.Frequency);
        Assert.Equal("cf2a797f-c8d8-470c-a736-6f5f78ae1d86", result.Account.Secret);
        Assert.Equal(2, result.Messages.Count);
    }

    [Fact]
    public async Task CreateSandboxAccountAsync_WritesTheSettlementWithEveryKey()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-account-sandbox/response-200-success.json"));

        await sdk.Accounts.CreateSandboxAccountAsync(NewWallet(new SettlementSettings { Currency = "GHS", Recipient = "0246798090" }));

        Assert.Equal(
            """{"type":1,"accountname":"My Business LTD","currency":"GHS","callback":"https://example.com/moolre/callback","settlement":{"currency":"GHS","frequency":"","channel":"","recipient":"0246798090","sublist":""}}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task CreateSandboxAccountAsync_InLive_ThrowsAndSendsNothing()
    {
        var options = TestTransport.SandboxOptions();
        options.Environment = MoolreEnvironment.Live;
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-account-sandbox/response-200-success.json"), options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sdk.Accounts.CreateSandboxAccountAsync(NewWallet()));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task CreateSandboxAccountAsync_WithACurrencyOtherThanGhs_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-account-sandbox/response-200-success.json"));
        var request = new CreateSandboxAccountRequest
        {
            AccountName = "X",
            Currency = Currency.Ngn,
            CallbackUrl = new Uri("https://example.com/cb"),
            Settlement = new SettlementSettings(),
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Accounts.CreateSandboxAccountAsync(request));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task CreateSandboxAccountAsync_ForADuplicateName_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"APGW21","message":"Account name already exists"}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Accounts.CreateSandboxAccountAsync(NewWallet()));

        Assert.Equal(MoolreResponseCode.DuplicateAccountName, exception.Code);
    }

    // ---- Update

    [Fact]
    public async Task UpdateAsync_ForTheDocumentedExample_ReturnsTheUpdatedWallet()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("update-account/response-200-success.json"));

        var result = await sdk.Accounts.UpdateAsync(new UpdateAccountRequest { AccountName = "My Business PLC" });

        Assert.Equal(MoolreResponseCode.AccountUpdated, result.Code);
        Assert.Equal("My Business PLC", result.Account.AccountName);
        Assert.Equal(new Uri("https://nocall.com/noend"), result.Account.CallbackUrl);
        Assert.Equal("0246798090", result.Account.Settlement.Recipient);
        Assert.Equal("300303", result.Account.Settlement.BankCode);
    }

    [Fact]
    public async Task UpdateAsync_SendsOnlyTheValuesThatWereSet()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("update-account/response-200-success.json"));

        await sdk.Accounts.UpdateAsync(new UpdateAccountRequest { CallbackUrl = new Uri("https://example.com/new") });

        // Compared by value: the order of fields in a JSON object does not matter to Moolre.
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"type":1,"accountnumber":"10772306069982","currency":"GHS","callback":"https://example.com/new"}"""),
            JsonNode.Parse(sdk.Handler.RequestBody!)));
    }

    [Fact]
    public async Task UpdateAsync_ForAnInvalidAccountNumber_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"WCU01","message":"Invalid account number"}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Accounts.UpdateAsync(new UpdateAccountRequest { AccountName = "X" }));

        Assert.Equal(MoolreResponseCode.UpdateInvalidAccountNumber, exception.Code);
    }

    // ---- Transactions

    [Fact]
    public async Task ListTransactionsAsync_ForTheDocumentedExample_MapsTheTransactions()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("list-transactions/response-200-success.json"));

        var result = await sdk.Accounts.ListTransactionsAsync();

        Assert.Equal(1, result.TransactionCount);
        var transaction = Assert.Single(result.Transactions);
        Assert.Equal("34008096", transaction.TransactionId);
        Assert.Equal(1.00m, transaction.Amount);
        Assert.Equal("""{"type":2,"accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ListTransactionsAsync_WithFilters_SendsGmtDatesTextLimitAndStatus()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("list-transactions/response-200-success.json"));

        await sdk.Accounts.ListTransactionsAsync(new ListAccountTransactionsRequest
        {
            From = new DateTimeOffset(2026, 1, 1, 2, 0, 0, TimeSpan.FromHours(2)),
            To = new DateTimeOffset(2026, 1, 31, 23, 59, 59, TimeSpan.Zero),
            Limit = 50,
            Status = TransactionStatus.Successful,
        });

        using var body = JsonDocument.Parse(sdk.Handler.RequestBody!);
        Assert.Equal("2026-01-01 00:00:00", body.RootElement.GetProperty("startdate").GetString());
        Assert.Equal("2026-01-31 23:59:59", body.RootElement.GetProperty("enddate").GetString());
        Assert.Equal("50", body.RootElement.GetProperty("limit").GetString());
        Assert.Equal(1, body.RootElement.GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData("""{"status":0,"code":"ST09","message":"No transactions"}""")]
    [InlineData("""{"status":1,"code":"ST09","message":"No transactions","data":null}""")]
    public async Task ListTransactionsAsync_WhenThereAreNone_ReturnsAnEmptyListNotAnError(string json)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(json));

        var result = await sdk.Accounts.ListTransactionsAsync();

        Assert.Empty(result.Transactions);
        Assert.Equal(0, result.TransactionCount);
    }

    [Fact]
    public async Task ListTransactionsAsync_WithAReversedRangeOrZeroLimit_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("list-transactions/response-200-success.json"));
        var reversed = new ListAccountTransactionsRequest { From = DateTimeOffset.UtcNow, To = DateTimeOffset.UtcNow.AddDays(-1) };
        var zeroLimit = new ListAccountTransactionsRequest { Limit = 0 };

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Accounts.ListTransactionsAsync(reversed));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Accounts.ListTransactionsAsync(zeroLimit));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Theory]
    [InlineData("/moolre/callback")]
    [InlineData("file:///etc/passwd")]
    public async Task CreateAndUpdate_WithACallbackThatIsNotAWebAddress_ThrowAndSendNothing(string address)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("update-account/response-200-success.json"));
        var callback = new Uri(address, UriKind.RelativeOrAbsolute);

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Accounts.CreateSandboxAccountAsync(
            new CreateSandboxAccountRequest { AccountName = "X", CallbackUrl = callback, Settlement = new SettlementSettings() }));
        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Accounts.UpdateAsync(new UpdateAccountRequest { CallbackUrl = callback }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }
}
