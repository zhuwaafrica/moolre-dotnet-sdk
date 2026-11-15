using Xunit;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>
/// Transfer calls against the real sandbox. The sandbox does not move real money. Each money call uses a reference unique to this
/// run, so reruns never collide.
/// </summary>
public sealed class TransfersSandboxTests
{
    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task ValidateNameAsync_ForTheTestNumber_AnswersFoundOrNotFound()
    {
        using var sandbox = new SandboxClient();
        var request = new ValidateNameRequest { Receiver = SandboxSettings.PhoneNumber!, Channel = TransferChannel.Mtn, Currency = Currency.Ghs };

        var result = await sandbox.Moolre.Transfers.ValidateNameAsync(request, TestContext.Current.CancellationToken);

        // Both are valid sandbox answers; a found number must come with a name and a missing one without.
        Assert.Equal(result.Found, result.AccountHolderName.Length > 0);
    }

    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task SendAsync_ThenGetStatusAsync_FindsTheTransferByItsReference()
    {
        using var sandbox = new SandboxClient();
        var reference = SandboxSettings.NewReference("transfer");
        var request = new SendTransferRequest
        {
            Channel = TransferChannel.Mtn,
            Currency = Currency.Ghs,
            Amount = 1m,
            Receiver = SandboxSettings.PhoneNumber!,
            ExternalReference = reference,
            Narration = "Zhuwa SDK integration test",
        };

        var sent = await sandbox.Moolre.Transfers.SendAsync(request, TestContext.Current.CancellationToken);
        var status = await sandbox.Moolre.Transfers.GetStatusAsync(
            new GetTransferStatusRequest { IdKind = TransactionIdKind.ExternalReference, Id = reference },
            TestContext.Current.CancellationToken);

        Assert.Equal(reference, sent.ExternalReference);
        Assert.Equal(reference, status.Transaction.ExternalReference);
    }

    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.ReceiverAccountNumber)]
    public async Task SendInternalAsync_ToTheSecondWallet_ReturnsATransferId()
    {
        using var sandbox = new SandboxClient();
        var request = new SendInternalTransferRequest
        {
            Currency = Currency.Ghs,
            Amount = 1m,
            ReceiverAccountNumber = SandboxSettings.ReceiverAccountNumber!,
            ExternalReference = SandboxSettings.NewReference("internal"),
        };

        var result = await sandbox.Moolre.Transfers.SendInternalAsync(request, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(result.TransferId));
    }
}
