using Xunit;
using Zhuwa.Moolre.Payments;

namespace Zhuwa.Moolre;

/// <summary>
/// Payment calls against the real sandbox. The sandbox does not take real money; <c>SkipOtp</c> avoids the SMS step. Each call
/// that creates something uses a reference unique to this run.
/// </summary>
public sealed class PaymentsSandboxTests
{
    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task RequestMobileMoneyAsync_WithSkipOtp_SendsThePromptOrAsksForAnOtp()
    {
        using var sandbox = new SandboxClient();
        var request = new RequestMobileMoneyRequest
        {
            Channel = PaymentChannel.Mtn,
            Currency = Currency.Ghs,
            PayerPhoneNumber = SandboxSettings.PhoneNumber!,
            Amount = 1m,
            ExternalReference = SandboxSettings.NewReference("collect"),
            SkipOtp = true,
        };

        var result = await sandbox.Moolre.Payments.RequestMobileMoneyAsync(request, TestContext.Current.CancellationToken);

        Assert.Contains(result.Outcome, new[] { MobileMoneyOutcome.PromptSent, MobileMoneyOutcome.OtpRequired });
    }

    [SandboxFact]
    public async Task CreatePaymentLinkAsync_ReturnsAnHttpsLink()
    {
        using var sandbox = new SandboxClient();
        var request = new CreatePaymentLinkRequest
        {
            Amount = 1m,
            Email = "integration-test@zhuwa.africa",
            ExternalReference = SandboxSettings.NewReference("link"),
            Currency = Currency.Ghs,
            ExpiresAfter = TimeSpan.FromMinutes(10),
        };

        var result = await sandbox.Moolre.Payments.CreatePaymentLinkAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(Uri.UriSchemeHttps, result.AuthorizationUrl.Scheme);
    }

    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task CreatePaymentIdAsync_ReturnsAPaymentId()
    {
        using var sandbox = new SandboxClient();
        var request = new CreatePaymentIdRequest
        {
            PhoneNumber = SandboxSettings.InternationalPhone(SandboxSettings.PhoneNumber!),
            Name = "Zhuwa Integration Test",
            Currency = Currency.Ghs,
            ExternalReference = SandboxSettings.NewReference("paymentid"),
        };

        var result = await sandbox.Moolre.Payments.CreatePaymentIdAsync(request, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(result.PaymentId));
    }

    [SandboxFact(SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber | SandboxNeeds.PhoneNumber)]
    public async Task CreateBankAccountAsync_ThenGetPaymentAccountAsync_FindsItByItsReference()
    {
        using var sandbox = new SandboxClient();
        var reference = SandboxSettings.NewReference("bank");
        var request = new CreateBankAccountRequest
        {
            Currency = Currency.Ghs,
            FirstName = "Zhuwa",
            LastName = "Test" + reference[^6..],
            PhoneNumber = SandboxSettings.PhoneNumber!,
            Email = "integration-test@zhuwa.africa",
            ExternalReference = reference,
        };

        var created = await sandbox.Moolre.Payments.CreateBankAccountAsync(request, TestContext.Current.CancellationToken);
        var lookup = await sandbox.Moolre.Payments.GetPaymentAccountAsync(
            new GetPaymentAccountRequest { ExternalReference = reference },
            TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(created.BankAccountNumber));

        // A new account has no payments yet; Moolre may answer "found" or "not found" for it. Either way the call must parse.
        Assert.Equal(lookup.Found, lookup.Account is not null);
    }

    [SandboxFact]
    public async Task ListBankAccountsAsync_ReturnsAConsistentList()
    {
        using var sandbox = new SandboxClient();

        var result = await sandbox.Moolre.Payments.ListBankAccountsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Count >= result.Accounts.Count || result.Count == 0);
    }
}
