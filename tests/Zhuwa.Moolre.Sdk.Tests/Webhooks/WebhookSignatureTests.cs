using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Zhuwa.Moolre.DependencyInjection;
using Zhuwa.Moolre.Webhooks;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests the <c>X-Moolre-Signature</c> check. The main cases come from Moolre's own test vectors (copied unchanged from
/// moolrehq/moolre-php, <c>tests/sdk/vectors/webhook.json</c>), so this SDK accepts and rejects exactly what Moolre's SDKs do.
/// </summary>
public sealed class WebhookSignatureTests
{
    private const long Now = 1763200000;

    private static readonly JsonElement Vectors = LoadVectors();

    private static JsonElement LoadVectors()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "payment-webhook", "signature-vectors.json");
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        return document.RootElement.Clone();
    }

    private static JsonElement Case(string name) =>
        Vectors.GetProperty("cases").EnumerateArray().Single(c => c.GetProperty("name").GetString() == name);

    /// <summary>
    /// Every vector except two whose settings this SDK refuses at startup instead of at verification: an empty secret (AddMoolreWebhooks
    /// requires one) and a tolerance of 0, which Moolre's PHP SDK reads as "no time check" and this SDK does not allow.
    /// </summary>
    public static TheoryData<string> VectorNames()
    {
        var data = new TheoryData<string>();
        foreach (var vector in LoadVectors().GetProperty("cases").EnumerateArray())
        {
            if (vector.GetProperty("secret").GetString() is { Length: > 0 } && vector.GetProperty("tolerance").GetInt32() > 0)
            {
                data.Add(vector.GetProperty("name").GetString()!);
            }
        }

        return data;
    }

    private static MoolreWebhookVerifier Verifier(Action<MoolreWebhookOptions> configure, long now = Now)
    {
        // Registered before AddMoolreWebhooks, so its TryAdd keeps this clock instead of the system one.
        var services = new ServiceCollection().AddSingleton<TimeProvider>(new FixedTimeProvider(DateTimeOffset.FromUnixTimeSeconds(now)));
        return services.AddMoolreWebhooks(configure).BuildServiceProvider().GetRequiredService<MoolreWebhookVerifier>();
    }

    /// <summary>Signs a body the way Moolre does, for the cases the vectors do not cover (several wallets).</summary>
    private static string Sign(string body, string secret, long timestamp = Now)
    {
        var signed = Encoding.UTF8.GetBytes(timestamp.ToString(CultureInfo.InvariantCulture) + "." + body);
        var hex = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), signed));
        return string.Create(CultureInfo.InvariantCulture, $"t={timestamp},v1={hex}");
    }

    [Theory]
    [MemberData(nameof(VectorNames))]
    public void IsSignatureValid_MoolreTestVector_GivesMoolresAnswer(string name)
    {
        var vector = Case(name);
        var verifier = Verifier(
            o =>
            {
                o.Secret = vector.GetProperty("secret").GetString();
                o.SignatureTolerance = TimeSpan.FromSeconds(vector.GetProperty("tolerance").GetInt32());
            },
            vector.GetProperty("now").GetInt64());

        var valid = verifier.IsSignatureValid(
            Encoding.UTF8.GetBytes(vector.GetProperty("body").GetString()!),
            vector.GetProperty("header").GetString());

        Assert.Equal(vector.GetProperty("valid").GetBoolean(), valid);
    }

    [Fact]
    public void IsSignatureValid_WithTheBodyAsText_MatchesTheByteOverload()
    {
        var vector = Case("valid_unicode_and_escaped_slash_body");
        var verifier = Verifier(o => o.Secret = vector.GetProperty("secret").GetString());

        Assert.True(verifier.IsSignatureValid(vector.GetProperty("body").GetString()!, vector.GetProperty("header").GetString()));
    }

    [Fact]
    public void IsSignatureValid_WithANullTextBody_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Verifier(o => o.Secret = "s").IsSignatureValid((string)null!, "t=1,v1=00"));
    }

    [Fact]
    public void IsSignatureValid_WhenNoSecretIsConfigured_ThrowsInsteadOfAccepting()
    {
        // Bypasses the startup validation on purpose, to prove the verifier itself never treats a missing setting as a match.
        var services = new ServiceCollection();
        services.AddOptions<MoolreWebhookOptions>();
        using var provider = services.BuildServiceProvider();
        var verifier = new DefaultMoolreWebhookVerifier(provider.GetRequiredService<IOptionsMonitor<MoolreWebhookOptions>>(), TimeProvider.System);

        Assert.Throws<InvalidOperationException>(() => verifier.IsSignatureValid("{}"u8, Sign("{}", "s")));
    }

    // ---- One secret per wallet: the wallet named in data.accountnumber selects the secret

    private const string WalletABody = """{"status":1,"code":"P01","data":{"txstatus":1,"accountnumber":"111","externalref":"R"}}""";

    private static MoolreWebhookVerifier WalletVerifier(string? fallback = null) => Verifier(o =>
    {
        o.Secret = fallback;
        o.WalletSecrets["111"] = "SECRET-A";
        o.WalletSecrets["222"] = "SECRET-B";
    });

    [Fact]
    public void IsSignatureValid_WithWalletSecrets_UsesTheSecretOfTheWalletInTheBody()
    {
        Assert.True(WalletVerifier().IsSignatureValid(WalletABody, Sign(WalletABody, "SECRET-A")));
    }

    [Fact]
    public void IsSignatureValid_SignedWithAnotherWalletsSecret_IsFalse()
    {
        Assert.False(WalletVerifier().IsSignatureValid(WalletABody, Sign(WalletABody, "SECRET-B")));
    }

    [Fact]
    public void IsSignatureValid_ForABodyNamingNoKnownWallet_UsesTheFallbackSecret()
    {
        // A webhook.test event is not a payment callback and names no wallet.
        const string TestEvent = """{"event":"webhook.test","event_id":"evt_aaaaaaaaaaaaaaaaaaaaaaaa"}""";

        Assert.True(WalletVerifier(fallback: "FALLBACK").IsSignatureValid(TestEvent, Sign(TestEvent, "FALLBACK")));
        Assert.False(WalletVerifier().IsSignatureValid(TestEvent, Sign(TestEvent, "SECRET-A")));
    }

    [Fact]
    public void IsSignatureValid_WithTheDefaultTolerance_AcceptsFiveMinutesAndRejectsOneSecondMore()
    {
        var verifier = Verifier(o => o.Secret = "s");

        Assert.True(verifier.IsSignatureValid(WalletABody, Sign(WalletABody, "s", Now - 300)));
        Assert.False(verifier.IsSignatureValid(WalletABody, Sign(WalletABody, "s", Now - 301)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddMoolreWebhooks_WithAToleranceThatIsNotPositive_FailsValidation(int seconds)
    {
        using var provider = new ServiceCollection()
            .AddMoolreWebhooks(o =>
            {
                o.Secret = "s";
                o.SignatureTolerance = TimeSpan.FromSeconds(seconds);
            })
            .BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MoolreWebhookOptions>>().Value);

        Assert.Contains("SignatureTolerance", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddMoolreWebhooks_WithoutATimeProvider_RegistersTheSystemClock()
    {
        using var provider = new ServiceCollection().AddMoolreWebhooks(o => o.Secret = "s").BuildServiceProvider();

        Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void SignatureHeaderName_IsTheHeaderMoolreDocuments()
    {
        Assert.Equal("X-Moolre-Signature", MoolreWebhookVerifier.SignatureHeaderName);
    }
}
