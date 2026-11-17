namespace Zhuwa.Moolre;

/// <summary>
/// Sandbox settings, read only from environment variables so credentials never live in a file in the repository
/// (the same approach as Octokit's integration tests). See tests/Zhuwa.Moolre.Sdk.IntegrationTests/README.md.
/// </summary>
internal static class SandboxSettings
{
    /// <summary>Opt-in switch: integration tests run only when this is <c>1</c>.</summary>
    internal static bool Enabled => Read("MOOLRE_SANDBOX_TESTS") == "1";

    /// <summary>The sandbox API user (<c>X-API-USER</c>).</summary>
    internal static string? ApiUser => Read("MOOLRE_SANDBOX_API_USER");

    /// <summary>The sandbox wallet the tests act on.</summary>
    internal static string? AccountNumber => Read("MOOLRE_SANDBOX_ACCOUNT_NUMBER");

    /// <summary>Optional private key; the sandbox does not check it, but some accounts may have one.</summary>
    internal static string? PrivateKey => Read("MOOLRE_SANDBOX_PRIVATE_KEY");

    /// <summary>Optional public key; the sandbox does not check it, but some accounts may have one.</summary>
    internal static string? PublicKey => Read("MOOLRE_SANDBOX_PUBLIC_KEY");

    /// <summary>The sandbox SMS service key (<c>X-API-VASKEY</c>).</summary>
    internal static string? SmsVasKey => Read("MOOLRE_SANDBOX_SMS_VASKEY");

    /// <summary>An approved SMS sender ID on the sandbox SMS service.</summary>
    internal static string? SmsSenderId => Read("MOOLRE_SANDBOX_SMS_SENDERID");

    /// <summary>The sandbox WhatsApp service key (<c>X-API-VASKEY</c>; a different key from SMS).</summary>
    internal static string? WhatsAppVasKey => Read("MOOLRE_SANDBOX_WHATSAPP_VASKEY");

    /// <summary>A mobile money number in local format (for example <c>0241234567</c>) used as payer and transfer receiver.</summary>
    internal static string? PhoneNumber => Read("MOOLRE_SANDBOX_PHONE");

    /// <summary>A second sandbox wallet, the receiver for internal transfers.</summary>
    internal static string? ReceiverAccountNumber => Read("MOOLRE_SANDBOX_RECEIVER_ACCOUNT_NUMBER");

    /// <summary>A reference that is unique for every run, so money calls never collide with an earlier run.</summary>
    internal static string NewReference(string prefix) => $"zhuwa-it-{prefix}-{Guid.NewGuid():N}";

    /// <summary>The phone number in the international form some endpoints need: <c>0241234567</c> becomes <c>+233241234567</c>.</summary>
    internal static string InternationalPhone(string localNumber) =>
        localNumber.StartsWith('+') ? localNumber : "+233" + localNumber.TrimStart('0');

    private static string? Read(string name) => Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;
}
