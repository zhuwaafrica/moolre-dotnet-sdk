using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zhuwa.Moolre.DependencyInjection;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre;

/// <summary>Tests the callback verifier: the secret check and the registration.</summary>
public sealed class WebhookVerifierTests
{
    private static MoolreWebhookVerifier Verifier(string? secret)
    {
        var provider = new ServiceCollection().AddMoolreWebhooks(options => options.Secret = secret).BuildServiceProvider();
        return provider.GetRequiredService<MoolreWebhookVerifier>();
    }

    private static PaymentWebhook CallbackWithSecret(string secret) =>
        PaymentWebhook.Parse("""{"status":1,"code":"P01","data":{"txstatus":1,"externalref":"R","secret":""" + "\"" + secret + "\"}}");

    [Fact]
    public void IsSecretValid_WhenTheSecretsMatch_IsTrue()
    {
        Assert.True(Verifier("YOUR_CALLBACK_SECRET").IsSecretValid(CallbackWithSecret("YOUR_CALLBACK_SECRET")));
    }

    [Theory]
    [InlineData("WRONG")]
    [InlineData("YOUR_CALLBACK_SECRE")]
    [InlineData("YOUR_CALLBACK_SECRET ")]
    [InlineData("your_callback_secret")]
    [InlineData("")]
    public void IsSecretValid_WhenTheSecretDiffersInAnyWay_IsFalse(string received)
    {
        Assert.False(Verifier("YOUR_CALLBACK_SECRET").IsSecretValid(CallbackWithSecret(received)));
    }

    [Fact]
    public void IsSecretValid_WhenTheCallbackHasNoSecret_IsFalse()
    {
        var webhook = PaymentWebhook.Parse("""{"status":1,"code":"P01","data":{"txstatus":1}}"""u8);

        Assert.False(Verifier("YOUR_CALLBACK_SECRET").IsSecretValid(webhook));
    }

    [Fact]
    public void IsSecretValid_WithANullWebhook_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Verifier("s").IsSecretValid(null!));
    }

    [Fact]
    public void AddMoolreWebhooks_WithoutASecret_FailsValidation()
    {
        using var provider = new ServiceCollection().AddMoolreWebhooks(_ => { }).BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreWebhookOptions>>().Value);

        Assert.Contains("Secret", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void IsSecretValid_WhenNoSecretIsConfigured_ThrowsInsteadOfAccepting()
    {
        // Bypasses the startup validation on purpose, to prove the verifier itself never treats a missing setting as a match.
        var services = new ServiceCollection();
        services.AddOptions<MoolreWebhookOptions>();
        services.AddSingleton<MoolreWebhookVerifier>(provider =>
            new DefaultMoolreWebhookVerifier(provider.GetRequiredService<IOptionsMonitor<MoolreWebhookOptions>>()));
        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MoolreWebhookVerifier>().IsSecretValid(CallbackWithSecret("x")));
    }

    // ---- One secret per wallet

    private static MoolreWebhookVerifier WalletVerifier(string? fallback = null)
    {
        var provider = new ServiceCollection().AddMoolreWebhooks(options =>
        {
            options.Secret = fallback;
            options.WalletSecrets["111"] = "SECRET-A";
            options.WalletSecrets["222"] = "SECRET-B";
        }).BuildServiceProvider();
        return provider.GetRequiredService<MoolreWebhookVerifier>();
    }

    private static PaymentWebhook Callback(string accountNumber, string secret) =>
        PaymentWebhook.Parse("""{"status":1,"code":"P01","data":{"txstatus":1,"accountnumber":""" + "\"" + accountNumber + "\",\"secret\":\"" + secret + "\"}}");

    [Fact]
    public void IsSecretValid_WithWalletSecrets_ChecksEachCallbackAgainstItsOwnWallet()
    {
        var verifier = WalletVerifier();

        Assert.True(verifier.IsSecretValid(Callback("111", "SECRET-A")));
        Assert.True(verifier.IsSecretValid(Callback("222", "SECRET-B")));
    }

    [Fact]
    public void IsSecretValid_WithAnotherWalletsSecret_IsFalse()
    {
        Assert.False(WalletVerifier().IsSecretValid(Callback("111", "SECRET-B")));
    }

    [Fact]
    public void IsSecretValid_ForAWalletWithNoSecretAndNoFallback_IsFalse()
    {
        Assert.False(WalletVerifier().IsSecretValid(Callback("999", "SECRET-A")));
    }

    [Fact]
    public void IsSecretValid_ForAWalletWithNoSecretOfItsOwn_UsesTheFallbackSecret()
    {
        var verifier = WalletVerifier(fallback: "FALLBACK");

        Assert.True(verifier.IsSecretValid(Callback("999", "FALLBACK")));
        Assert.False(verifier.IsSecretValid(Callback("111", "FALLBACK")));
    }

    [Fact]
    public void AddMoolreWebhooks_WithOnlyWalletSecrets_PassesValidation()
    {
        using var provider = new ServiceCollection().AddMoolreWebhooks(o => o.WalletSecrets["111"] = "SECRET-A").BuildServiceProvider();

        Assert.Equal("SECRET-A", provider.GetRequiredService<IOptions<MoolreWebhookOptions>>().Value.WalletSecrets["111"]);
    }

    [Fact]
    public void AddMoolreWebhooks_WithABlankWalletSecret_FailsValidation()
    {
        using var provider = new ServiceCollection().AddMoolreWebhooks(o => o.WalletSecrets["111"] = string.Empty).BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreWebhookOptions>>().Value);
    }
}
