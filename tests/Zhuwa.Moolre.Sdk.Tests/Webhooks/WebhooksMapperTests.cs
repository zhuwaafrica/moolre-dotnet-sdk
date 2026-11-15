using System.Text;
using Xunit;

namespace Zhuwa.Moolre;

/// <summary>Tests reading Moolre's payment callback bodies: the documented success and failure callbacks, and malformed bodies.</summary>
public sealed class WebhooksMapperTests
{
    private static byte[] Fixture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "payment-webhook", name));

    [Fact]
    public void Parse_TheDocumentedSuccessCallback_ReadsEveryFieldAndIsASuccessfulPayment()
    {
        var webhook = PaymentWebhook.Parse(Fixture("successful-payment-callback.json"));

        Assert.True(webhook.IsSuccessfulPayment);
        Assert.Equal(MoolreResponseCode.PaymentSucceeded, webhook.Code);
        Assert.Equal("Transaction Successful", webhook.Message);
        Assert.Equal(TransactionStatus.Successful, webhook.Data.Status);
        Assert.Equal("233242097401", webhook.Data.Payer);
        Assert.Equal("BISMARK AMOAH", webhook.Data.PayerName);
        Assert.Equal("MTN", webhook.Data.PayerNetwork);
        Assert.Equal(101m, webhook.Data.Amount);
        Assert.Equal(98.97m, webhook.Data.Value);
        Assert.Equal("54214408", webhook.Data.TransactionId);
        Assert.Equal("CDT20261111F1CE1703", webhook.Data.ExternalReference);
        Assert.Equal("YOUR_CALLBACK_SECRET", webhook.Data.Secret);
        Assert.Equal(new DateTimeOffset(2026, 11, 11, 12, 35, 49, TimeSpan.Zero), webhook.Data.Timestamp);
        Assert.Null(webhook.Data.Reason);
    }

    [Fact]
    public void Parse_TheDocumentedFailedCallback_IsNeverASuccessfulPaymentAndKeepsTheReason()
    {
        var webhook = PaymentWebhook.Parse(Fixture("failed-payment-callback.json"));

        Assert.False(webhook.IsSuccessfulPayment);
        Assert.Equal(MoolreResponseCode.PaymentFailed, webhook.Code);
        Assert.Equal(TransactionStatus.Failed, webhook.Data.Status);
        Assert.Null(webhook.Data.TransactionId);
        Assert.Null(webhook.Data.PayerName);
        Assert.Equal("LOW_BALANCE_OR_PAYEE_LIMIT_REACHED_OR_NOT_ALLOWED", webhook.Data.Reason);
        Assert.Equal(0m, webhook.Data.Amount);
    }

    [Fact]
    public void Parse_FromText_ReadsTheSameCallback()
    {
        var webhook = PaymentWebhook.Parse(Encoding.UTF8.GetString(Fixture("successful-payment-callback.json")));

        Assert.Equal("CDT20261111F1CE1703", webhook.Data.ExternalReference);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("""{"status":1,"code":"P01","message":"ok"}""")]
    [InlineData("""{"status":1,"code":"P01","data":"oops"}""")]
    public void Parse_ABodyThatIsNotACallback_ThrowsResponseException(string body)
    {
        Assert.Throws<MoolreResponseException>(() => PaymentWebhook.Parse(Encoding.UTF8.GetBytes(body)));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"status":1,"code":"P01"}""")]
    [InlineData(null)]
    public void TryParse_ABodyThatIsNotACallback_ReturnsFalse(string? body)
    {
        var parsed = PaymentWebhook.TryParse(body, out var webhook);

        Assert.False(parsed);
        Assert.Null(webhook);
    }

    [Fact]
    public void TryParse_ARealCallback_ReturnsTrue()
    {
        var parsed = PaymentWebhook.TryParse(Fixture("successful-payment-callback.json"), out var webhook);

        Assert.True(parsed);
        Assert.NotNull(webhook);
    }

    [Fact]
    public void Parse_ASuccessCodeWithAFailedTransaction_IsNotASuccessfulPayment()
    {
        var body = """{"status":1,"code":"P01","data":{"txstatus":2,"externalref":"R","secret":"s"}}""";

        var webhook = PaymentWebhook.Parse(Encoding.UTF8.GetBytes(body));

        Assert.False(webhook.IsSuccessfulPayment);
    }

    [Fact]
    public void Parse_ACallbackWithoutASecret_ReadsAnEmptySecret()
    {
        var webhook = PaymentWebhook.Parse("""{"status":1,"code":"P01","data":{"txstatus":1}}"""u8);

        Assert.Equal(string.Empty, webhook.Data.Secret);
    }
}
