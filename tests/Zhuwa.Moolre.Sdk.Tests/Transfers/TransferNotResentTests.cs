using System.Net;
using Xunit;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>
/// The money-safety rules for transfers: a transfer is sent exactly once, and when its outcome is unknown the caller gets the
/// reference back to look it up. These tests guard against paying twice.
/// </summary>
public sealed class TransferNotResentTests
{
    private static SendTransferRequest Transfer(string reference) => new()
    {
        Channel = TransferChannel.Mtn,
        Currency = Currency.Ghs,
        Amount = 50m,
        Receiver = "0246798993",
        ExternalReference = reference,
    };

    private static SendInternalTransferRequest InternalTransfer(string reference) => new()
    {
        Currency = Currency.Ghs,
        Amount = 50m,
        ReceiverAccountNumber = "10000010002",
        ExternalReference = reference,
    };

    private static TestTransport TimingOut()
    {
        var options = TestTransport.SandboxOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        return TestTransport.Create(StubHttpHandler.NeverAnswering(), options);
    }

    [Fact]
    public async Task SendAsync_WhenItTimesOut_ThrowsUncertainWithTheReferenceAndSendsOnce()
    {
        var sdk = TimingOut();

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendAsync(Transfer("T-9001")));

        Assert.Equal("T-9001", exception.ExternalReference);
        Assert.Equal("Transfers.SendAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheConnectionFails_ThrowsUncertainAndSendsOnce()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("connection reset")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendAsync(Transfer("T-9002")));

        Assert.Equal("T-9002", exception.ExternalReference);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTheAnswerIsUnreadable_ThrowsUncertain()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("<html>502 Bad Gateway</html>", HttpStatusCode.BadGateway));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendAsync(Transfer("T-9003")));

        Assert.Equal("T-9003", exception.ExternalReference);
    }

    [Fact]
    public async Task SendAsync_TrimsTheReferenceAndReportsTheTrimmedValue()
    {
        var sdk = TimingOut();

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendAsync(Transfer("  T-9004  ")));

        Assert.Equal("T-9004", exception.ExternalReference);
    }

    [Fact]
    public async Task SendAsync_ForADuplicateReference_ThrowsDuplicateNotUncertain()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"APY18","message":"External reference must be unique","data":"externalref"}"""));

        await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => sdk.Transfers.SendAsync(Transfer("T-9005")));

        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendInternalAsync_WhenItTimesOut_ThrowsUncertainWithTheReferenceAndSendsOnce()
    {
        var sdk = TimingOut();

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendInternalAsync(InternalTransfer("I-9001")));

        Assert.Equal("I-9001", exception.ExternalReference);
        Assert.Equal("Transfers.SendInternalAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task ValidateNameAsync_WhenItTimesOut_ThrowsAPlainTimeoutBecauseItIsSafeToRepeat()
    {
        var sdk = TimingOut();

        await Assert.ThrowsAsync<TimeoutException>(() => sdk.Transfers.ValidateNameAsync(new ValidateNameRequest
        {
            Receiver = "0246798993",
            Channel = TransferChannel.Mtn,
            Currency = Currency.Ghs,
        }));
    }

    [Fact]
    public async Task GetStatusAsync_WhenTheConnectionFails_LetsTheErrorThroughSoTheCallerCanRetry()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("connection reset")));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sdk.Transfers.GetStatusAsync(new GetTransferStatusRequest { IdKind = TransactionIdKind.ExternalReference, Id = "T-1" }));
    }
}
