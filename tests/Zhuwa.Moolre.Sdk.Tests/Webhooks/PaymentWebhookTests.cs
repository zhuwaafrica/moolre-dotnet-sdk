using Xunit;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre;

public sealed class PaymentWebhookTests
{
    private static PaymentWebhookData CreateData(TransactionStatus status) =>
        new(
            status,
            payer: "233242097401",
            payerName: null,
            payerNetwork: null,
            payerInstitution: null,
            terminalId: string.Empty,
            accountNumber: "10772306069982",
            name: string.Empty,
            amount: 101m,
            value: 98.97m,
            transactionId: status == TransactionStatus.Failed ? null : "54214408",
            externalReference: "ORDER-1",
            thirdPartyReference: string.Empty,
            reason: null,
            secret: "EXPECTED-SECRET",
            timestamp: new DateTimeOffset(2026, 11, 11, 12, 35, 49, TimeSpan.Zero));

    private static PaymentWebhook CreateWebhook(MoolreResponseCode code, TransactionStatus status, bool wireStatusIsSuccess) =>
        new(code, "message", CreateData(status), wireStatusIsSuccess);

    [Fact]
    public void IsSuccessfulPayment_WhenStatusCodeAndTransactionStatusAllAgree_IsTrue()
    {
        var webhook = CreateWebhook(MoolreResponseCode.PaymentSucceeded, TransactionStatus.Successful, wireStatusIsSuccess: true);

        Assert.True(webhook.IsSuccessfulPayment);
    }

    [Fact]
    public void IsSuccessfulPayment_ForAFailedCallback_IsFalse()
    {
        var webhook = CreateWebhook(MoolreResponseCode.PaymentFailed, TransactionStatus.Failed, wireStatusIsSuccess: false);

        Assert.False(webhook.IsSuccessfulPayment);
    }

    [Fact]
    public void IsSuccessfulPayment_WhenOnlyTheCodeSaysSuccess_IsFalse()
    {
        var webhook = CreateWebhook(MoolreResponseCode.PaymentSucceeded, TransactionStatus.Pending, wireStatusIsSuccess: true);

        Assert.False(webhook.IsSuccessfulPayment);
    }

    [Fact]
    public void IsSuccessfulPayment_WhenTheWireStatusIsNotOne_IsFalse()
    {
        var webhook = CreateWebhook(MoolreResponseCode.PaymentSucceeded, TransactionStatus.Successful, wireStatusIsSuccess: false);

        Assert.False(webhook.IsSuccessfulPayment);
    }

    [Fact]
    public void Data_ForAFailedPayment_AllowsANullTransactionId()
    {
        var webhook = CreateWebhook(MoolreResponseCode.PaymentFailed, TransactionStatus.Failed, wireStatusIsSuccess: false);

        Assert.Null(webhook.Data.TransactionId);
        Assert.Equal("ORDER-1", webhook.Data.ExternalReference);
    }
}
