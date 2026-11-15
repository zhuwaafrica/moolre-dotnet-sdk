using System.Net;
using System.Text.Json;
using Xunit;
using Zhuwa.Moolre.Payments;

namespace Zhuwa.Moolre;

/// <summary>Tests the payment calls through the stub handler: what is sent, how answers become results, and which answers throw.</summary>
public sealed class PaymentsTests
{
    // ---- Status

    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_MapsTheTransactionAndUsesThePublicKey()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("payment-status/response-200-success.json"));

        var result = await sdk.Payments.GetStatusAsync(new GetPaymentStatusRequest { IdKind = TransactionIdKind.ExternalReference, Id = "1231231-128" });

        Assert.Equal(TransactionStatus.Successful, result.Transaction.Status);
        Assert.Equal("31772290", result.Transaction.TransactionId);
        Assert.Equal("""{"type":1,"idtype":1,"id":"1231231-128","accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
        Assert.Equal("public-key", sdk.Handler.Request!.Headers.GetValues("X-API-PUBKEY").Single());
        Assert.False(sdk.Handler.Request.Headers.Contains("X-API-KEY"));
    }

    [Fact]
    public async Task GetStatusAsync_WithoutAnId_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("payment-status/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sdk.Payments.GetStatusAsync(new GetPaymentStatusRequest { IdKind = TransactionIdKind.ExternalReference, Id = " " }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Payment ID

    [Fact]
    public async Task CreatePaymentIdAsync_ForTheDocumentedExample_ReturnsThePaymentId()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-payment-id/response-200-success.json"));

        var result = await sdk.Payments.CreatePaymentIdAsync(new CreatePaymentIdRequest
        {
            PhoneNumber = "+233267606822",
            Name = "Kofi Adeniyi",
            Currency = Currency.Ghs,
            ExternalReference = "PID-1",
        });

        Assert.Equal("08160984", result.PaymentId);
        Assert.Equal("Kofi Adeniyi", result.Name);
        Assert.Null(result.QrCode);
        // The default JSON writer escapes "+" as \u002B; that is valid JSON and decodes to "+", so the body is compared by value.
        using var body = JsonDocument.Parse(sdk.Handler.RequestBody!);
        Assert.Equal(2, body.RootElement.GetProperty("type").GetInt32());
        Assert.Equal("+233267606822", body.RootElement.GetProperty("phone").GetString());
        Assert.Equal("Kofi Adeniyi", body.RootElement.GetProperty("name").GetString());
        Assert.Equal("GHS", body.RootElement.GetProperty("currency").GetString());
        Assert.Equal("PID-1", body.RootElement.GetProperty("externalref").GetString());
        Assert.False(body.RootElement.TryGetProperty("amount", out _));
    }

    [Fact]
    public async Task CreatePaymentIdAsync_WithAPresetAmount_SendsItAsText()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-payment-id/response-200-success.json"));

        await sdk.Payments.CreatePaymentIdAsync(new CreatePaymentIdRequest
        {
            PhoneNumber = "+233267606822",
            Name = "Kofi",
            Currency = Currency.Ghs,
            Amount = 20m,
            ExternalReference = "PID-2",
        });

        Assert.Contains("\"amount\":\"20\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatePaymentIdAsync_WithALocalPhoneNumber_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-payment-id/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Payments.CreatePaymentIdAsync(new CreatePaymentIdRequest
        {
            PhoneNumber = "0267606822",
            Name = "Kofi",
            Currency = Currency.Ghs,
            ExternalReference = "PID-3",
        }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Bank collections

    [Fact]
    public async Task CreateBankAccountAsync_ForTheDocumentedExample_ReturnsTheAccount()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("bank-collections/response-200-success.json"));

        var result = await sdk.Payments.CreateBankAccountAsync(BankAccountRequest(amount: 50m));

        Assert.Equal("0014730001001", result.BankAccountNumber);
        Assert.Equal("First Atlantic Bank", result.BankName);
        Assert.Equal("BANK-1001", result.ExternalReference);
        Assert.Equal(
            """{"type":9,"currency":"GHS","amount":50,"firstname":"Kofi","lastname":"Adeniyi","phone":"0244000000","email":"kofi@example.com","externalref":"BANK-1001","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task CreateBankAccountAsync_WhenMoolreCannotCreateIt_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("bank-collections/response-400-duplicate-name-failed.json", HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Payments.CreateBankAccountAsync(BankAccountRequest()));

        Assert.Equal(MoolreResponseCode.BankAccountCreationFailed, exception.Code);
    }

    [Fact]
    public async Task ListBankAccountsAsync_ForTheDocumentedExample_MapsEveryAccount()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("list-bank-accounts/response-200-success.json"));

        var result = await sdk.Payments.ListBankAccountsAsync();

        Assert.Equal(4, result.Count);
        Assert.Equal(4, result.Accounts.Count);
        Assert.Equal("0052080001001", result.Accounts[1].BankAccountNumber);
        Assert.Equal("ur12ef0012", result.Accounts[1].ExternalReference);
        Assert.Equal(new DateTimeOffset(2026, 9, 9, 11, 52, 56, TimeSpan.Zero), result.Accounts[1].CreatedAt);
        Assert.Equal("""{"type":3,"accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    // ---- Payment ID / bank account lookup

    [Theory]
    [InlineData("payment-id-bank-account-status/response-200-found-found.json")]
    [InlineData("list-payment-id-transactions/response-200-found-found.json")]
    [InlineData("list-bank-account-transactions/response-200-found-found.json")]
    public async Task GetPaymentAccountAsync_WhenFound_ReadsTheAccountAndTransactionsWhetherOneOrMany(string fixture)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture(fixture));

        var result = await sdk.Payments.GetPaymentAccountAsync(new GetPaymentAccountRequest { ExternalReference = "ur12ef001" });

        Assert.True(result.Found);
        Assert.Equal("0052030001001", result.Account!.BankAccountNumber);
        Assert.Equal(1, result.Account.TransactionCount);
        Assert.Equal(4.60m, result.Account.TransactionValue);
        var transaction = Assert.Single(result.Transactions);
        Assert.Equal(0.40m, transaction.Fee);
        Assert.Equal("""{"type":2,"externalref":"ur12ef001","accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Theory]
    [InlineData("payment-id-bank-account-status/response-200-not-found-not-found.json")]
    [InlineData("list-payment-id-transactions/response-200-not-found-not-found.json")]
    public async Task GetPaymentAccountAsync_WhenNotFound_ReturnsANotFoundResult(string fixture)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture(fixture));

        var result = await sdk.Payments.GetPaymentAccountAsync(new GetPaymentAccountRequest { ExternalReference = "missing" });

        Assert.False(result.Found);
        Assert.Null(result.Account);
        Assert.Empty(result.Transactions);
    }

    private static CreateBankAccountRequest BankAccountRequest(decimal? amount = null) => new()
    {
        Currency = Currency.Ghs,
        Amount = amount,
        FirstName = "Kofi",
        LastName = "Adeniyi",
        PhoneNumber = "0244000000",
        Email = "kofi@example.com",
        ExternalReference = "BANK-1001",
    };
}
