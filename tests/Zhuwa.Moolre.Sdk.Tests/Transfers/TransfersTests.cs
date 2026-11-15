using System.Net;
using Xunit;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>Tests the transfer calls through the stub handler: what is sent, how answers become results, and which answers throw.</summary>
public sealed class TransfersTests
{
    private static SendTransferRequest MobileMoneyTransfer(decimal amount = 1m, string reference = "T-1001") => new()
    {
        Channel = TransferChannel.Mtn,
        Currency = Currency.Ghs,
        Amount = amount,
        Receiver = "0246798993",
        ExternalReference = reference,
    };

    private static ValidateNameRequest MobileMoneyName() => new()
    {
        Receiver = "0246798993",
        Channel = TransferChannel.Mtn,
        Currency = Currency.Ghs,
    };

    // ---- Validate name

    [Fact]
    public async Task ValidateNameAsync_ForTheDocumentedExample_ReturnsTheName()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("validate-name/response-200-success.json"));

        var result = await sdk.Transfers.ValidateNameAsync(MobileMoneyName());

        Assert.True(result.Found);
        Assert.Equal("BRIGHT BUAME", result.AccountHolderName);
        Assert.Equal(
            """{"type":1,"receiver":"0246798993","channel":"1","currency":"GHS","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ValidateNameAsync_WhenTheNumberIsNotFound_ReturnsANotFoundResultWithoutAName()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("validate-name/response-400-not-found.json", HttpStatusCode.BadRequest));

        var result = await sdk.Transfers.ValidateNameAsync(MobileMoneyName());

        Assert.False(result.Found);
        Assert.Equal(string.Empty, result.AccountHolderName);
        Assert.Equal(MoolreResponseCode.NameNotFound, result.Code);
    }

    [Fact]
    public async Task ValidateNameAsync_UsesEitherKeyAndPrefersThePrivateKey()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("validate-name/response-200-success.json"));

        await sdk.Transfers.ValidateNameAsync(MobileMoneyName());

        Assert.Equal("private-key", sdk.Handler.Request!.Headers.GetValues("X-API-KEY").Single());
    }

    [Fact]
    public async Task ValidateNameAsync_ForABankAccount_SendsTheBankCode()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("validate-name/response-200-success.json"));

        await sdk.Transfers.ValidateNameAsync(new ValidateNameRequest
        {
            Receiver = "1234567890",
            Channel = TransferChannel.Bank,
            BankCode = "300303",
            Currency = Currency.Ghs,
        });

        Assert.Contains("\"channel\":\"2\",\"sublistid\":\"300303\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateNameAsync_ForABankAccountWithoutABankCode_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("validate-name/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Transfers.ValidateNameAsync(new ValidateNameRequest
        {
            Receiver = "1234567890",
            Channel = TransferChannel.Bank,
            Currency = Currency.Ghs,
        }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task ValidateNameAsync_ForAnyOtherFailure_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"XX99","message":"Something else"}"""));

        await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Transfers.ValidateNameAsync(MobileMoneyName()));
    }

    // ---- Send

    [Fact]
    public async Task SendAsync_ForTheDocumentedExample_MapsTheTransfer()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

        var result = await sdk.Transfers.SendAsync(MobileMoneyTransfer());

        Assert.Equal(MoolreResponseCode.PayoutSuccessful, result.Code);
        Assert.Equal(TransactionStatus.Successful, result.Status);
        Assert.Equal("32759150", result.TransactionId);
        Assert.Equal("28171451", result.ExternalReference);
        Assert.Equal("YUSIF YA-ADZAGEY", result.ReceiverName);
        Assert.Equal(1m, result.Amount);
        Assert.Equal(1.01m, result.AmountWithFee);
        Assert.Equal(0.01m, result.Fee);
        Assert.Equal(["Pay out Successful", "Click close to view transactions."], result.Messages);
    }

    [Fact]
    public async Task SendAsync_WritesEveryFieldAsMoolreExpects()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

        await sdk.Transfers.SendAsync(new SendTransferRequest
        {
            Channel = TransferChannel.Telecel,
            Currency = Currency.Ghs,
            Amount = 10.5m,
            Receiver = " 0201234567 ",
            ExternalReference = "T-2002",
            Narration = "Salary",
        });

        Assert.Equal(
            """{"type":1,"channel":"6","currency":"GHS","amount":"10.5","receiver":"0201234567","externalref":"T-2002","reference":"Salary","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task SendAsync_UsesThePrivateKeyOnly()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

        await sdk.Transfers.SendAsync(MobileMoneyTransfer());

        Assert.Equal("private-key", sdk.Handler.Request!.Headers.GetValues("X-API-KEY").Single());
        Assert.False(sdk.Handler.Request.Headers.Contains("X-API-PUBKEY"));
    }

    [Fact]
    public async Task SendAsync_WhenThePayoutIsInProgress_ReturnsPendingNotAnError()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"OTGH02","message":"Pay out in Progress","data":null}"""));

        var result = await sdk.Transfers.SendAsync(MobileMoneyTransfer(reference: "T-3003"));

        Assert.Equal(TransactionStatus.Pending, result.Status);
        Assert.Equal("T-3003", result.ExternalReference);
    }

    [Fact]
    public async Task SendAsync_WhenTheAnswerHasNoStatus_ReportsPendingNeverSuccessful()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"OBGH01","message":"ok","data":{"transactionid":"9"}}"""));

        var result = await sdk.Transfers.SendAsync(MobileMoneyTransfer());

        Assert.Equal(TransactionStatus.Pending, result.Status);
    }

    [Theory]
    [InlineData("APY17", "Pay out failed")]
    [InlineData("APY21", "Insufficient wallet balance")]
    public async Task SendAsync_WhenMoolreRefusesTheTransfer_ThrowsApiExceptionWithTheCode(string code, string message)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning($$"""{"status":0,"code":"{{code}}","message":"{{message}}","data":null}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Transfers.SendAsync(MobileMoneyTransfer()));

        Assert.Equal(code, exception.Code.Value);
        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.005)]
    public async Task SendAsync_WithAnInvalidAmount_ThrowsAndSendsNothing(double amount)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Transfers.SendAsync(MobileMoneyTransfer((decimal)amount)));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithoutAnExternalReference_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Transfers.SendAsync(MobileMoneyTransfer(reference: " ")));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithoutAChannelOrCurrency_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));
        var noChannel = new SendTransferRequest { Channel = TransferChannel.None, Currency = Currency.Ghs, Amount = 1m, Receiver = "0", ExternalReference = "T" };
        var noCurrency = new SendTransferRequest { Channel = TransferChannel.Mtn, Currency = Currency.None, Amount = 1m, Receiver = "0", ExternalReference = "T" };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Transfers.SendAsync(noChannel));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sdk.Transfers.SendAsync(noCurrency));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_ToAMobileNumber_NeverSendsABankCode()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));
        var request = new SendTransferRequest
        {
            Channel = TransferChannel.Mtn,
            Currency = Currency.Ghs,
            Amount = 1m,
            Receiver = "0246798993",
            ExternalReference = "T-4",
            BankCode = "300303",
        };

        await sdk.Transfers.SendAsync(request);

        Assert.DoesNotContain("sublistid", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    // ---- Status

    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_MapsTheTransaction()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("transfer-status/response-200-success.json"));

        var result = await sdk.Transfers.GetStatusAsync(new GetTransferStatusRequest { IdKind = TransactionIdKind.ExternalReference, Id = "1231231-12985" });

        Assert.Equal(TransactionStatus.Successful, result.Transaction.Status);
        Assert.Equal("31830714", result.Transaction.TransactionId);
        Assert.Equal(5m, result.Transaction.Amount);
        Assert.Equal(new DateTimeOffset(2024, 1, 5, 9, 42, 33, TimeSpan.Zero), result.Transaction.Timestamp);
        Assert.Equal("""{"type":1,"idtype":1,"id":"1231231-12985","accountnumber":"10772306069982"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task GetStatusAsync_ByMoolreId_SendsIdTypeTwo()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("transfer-status/response-200-success.json"));

        await sdk.Transfers.GetStatusAsync(new GetTransferStatusRequest { IdKind = TransactionIdKind.MoolreId, Id = "31830714" });

        Assert.Contains("\"idtype\":2", sdk.Handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetStatusAsync_WithoutAnIdKind_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("transfer-status/response-200-success.json"));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sdk.Transfers.GetStatusAsync(new GetTransferStatusRequest { IdKind = TransactionIdKind.None, Id = "1" }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    // ---- Internal transfer

    [Fact]
    public async Task SendInternalAsync_ForTheDocumentedExample_ReturnsTheTransferId()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("internal-transfer/response-200-success.json"));

        var result = await sdk.Transfers.SendInternalAsync(new SendInternalTransferRequest
        {
            Currency = Currency.Ghs,
            Amount = 25m,
            ReceiverAccountNumber = "10000010002",
            ExternalReference = "I-1",
        });

        Assert.Equal("f25fc80e-791b-495b-8799-dcf87660457d", result.TransferId);
        Assert.Equal(
            """{"type":1,"currency":"GHS","amount":"25","receiver":"10000010002","externalref":"I-1","accountnumber":"10772306069982"}""",
            sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task SendInternalAsync_ForADuplicateReference_ThrowsDuplicateReferenceException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("internal-transfer/response-400-duplicate-reference.json", HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => sdk.Transfers.SendInternalAsync(new SendInternalTransferRequest
        {
            Currency = Currency.Ghs,
            Amount = 25m,
            ReceiverAccountNumber = "10000010002",
            ExternalReference = "I-1",
        }));

        Assert.Equal("externalref", exception.Detail);
    }

    [Fact]
    public async Task SendInternalAsync_ForInsufficientBalance_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"TR09","message":"Insufficient balance"}"""));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Transfers.SendInternalAsync(new SendInternalTransferRequest
        {
            Currency = Currency.Ghs,
            Amount = 25m,
            ReceiverAccountNumber = "10000010002",
            ExternalReference = "I-2",
        }));

        Assert.Equal(MoolreResponseCode.InternalTransferInsufficientBalance, exception.Code);
    }
}
