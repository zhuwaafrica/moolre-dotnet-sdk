using Xunit;
using Zhuwa.Moolre.Sms;

namespace Zhuwa.Moolre;

/// <summary>SMS against the real sandbox, with the SMS service's own key. The send test uses a message reference unique to the run.</summary>
public sealed class SmsSandboxTests
{
    [SandboxFact(SandboxNeeds.SmsKey)]
    public async Task GetBalanceAsync_ReturnsABalance()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.Sms.GetBalanceAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Balance >= 0m);
    }

    [SandboxFact(SandboxNeeds.SmsKey)]
    public async Task ListSenderIdsAsync_ReturnsSenderIdsWithAnApproval()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.Sms.ListSenderIdsAsync(TestContext.Current.CancellationToken);

        Assert.All(result.SenderIds, s => Assert.False(string.IsNullOrWhiteSpace(s.SenderId)));
    }

    [SandboxFact(SandboxNeeds.SmsKey | SandboxNeeds.SmsSenderId | SandboxNeeds.PhoneNumber)]
    public async Task SendAsync_ThenGetStatusAsync_FindsTheMessageByItsReference()
    {
        using var sandbox = new SandboxClient();
        var reference = SandboxSettings.NewReference("sms");
        var send = new SendSmsRequest { SenderId = SandboxSettings.SmsSenderId! };
        send.Messages.Add(new SmsMessage { Recipient = SandboxSettings.PhoneNumber!, Text = "Zhuwa SDK integration test", Reference = reference });

        var sent = await sandbox.Moolre.Sms.SendAsync(send, TestContext.Current.CancellationToken);
        var lookup = new GetSmsStatusRequest();
        lookup.References.Add(reference);
        var status = await sandbox.Moolre.Sms.GetStatusAsync(lookup, TestContext.Current.CancellationToken);

        Assert.Equal(MoolreResponseCode.SmsSent, sent.Code);
        Assert.All(status.Statuses, s => Assert.Equal(reference, s.Reference));
    }
}
