using System.Runtime.CompilerServices;
using Xunit;

namespace Zhuwa.Moolre;

/// <summary>
/// A test that calls the real sandbox. It is skipped, with a message naming what is missing, unless the integration tests are
/// switched on and every setting the test needs is present. Octokit marks its integration tests the same way.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class SandboxFactAttribute : FactAttribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="needs">Which settings the test needs beyond the opt-in switch.</param>
    /// <param name="sourceFilePath">Filled in by the compiler; passed to xUnit so the test shows its source location.</param>
    /// <param name="sourceLineNumber">Filled in by the compiler.</param>
    public SandboxFactAttribute(
        SandboxNeeds needs = SandboxNeeds.ApiUser | SandboxNeeds.AccountNumber,
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        var missing = new List<string>();
        if (!SandboxSettings.Enabled)
        {
            missing.Add("MOOLRE_SANDBOX_TESTS=1");
        }

        if (needs.HasFlag(SandboxNeeds.ApiUser) && SandboxSettings.ApiUser is null)
        {
            missing.Add("MOOLRE_SANDBOX_API_USER");
        }

        if (needs.HasFlag(SandboxNeeds.AccountNumber) && SandboxSettings.AccountNumber is null)
        {
            missing.Add("MOOLRE_SANDBOX_ACCOUNT_NUMBER");
        }

        if (needs.HasFlag(SandboxNeeds.PhoneNumber) && SandboxSettings.PhoneNumber is null)
        {
            missing.Add("MOOLRE_SANDBOX_PHONE");
        }

        if (needs.HasFlag(SandboxNeeds.ReceiverAccountNumber) && SandboxSettings.ReceiverAccountNumber is null)
        {
            missing.Add("MOOLRE_SANDBOX_RECEIVER_ACCOUNT_NUMBER");
        }

        if (needs.HasFlag(SandboxNeeds.SmsKey) && SandboxSettings.SmsVasKey is null)
        {
            missing.Add("MOOLRE_SANDBOX_SMS_VASKEY");
        }

        if (needs.HasFlag(SandboxNeeds.SmsSenderId) && SandboxSettings.SmsSenderId is null)
        {
            missing.Add("MOOLRE_SANDBOX_SMS_SENDERID");
        }

        if (missing.Count > 0)
        {
            Skip = "Sandbox test. Set " + string.Join(", ", missing) + " (see tests/Zhuwa.Moolre.Sdk.IntegrationTests/README.md).";
        }
    }
}

/// <summary>The settings a sandbox test needs, beyond the opt-in switch.</summary>
[Flags]
internal enum SandboxNeeds
{
    /// <summary>Nothing else (calls that need no credentials).</summary>
    None = 0,

    /// <summary><c>MOOLRE_SANDBOX_API_USER</c>.</summary>
    ApiUser = 1,

    /// <summary><c>MOOLRE_SANDBOX_ACCOUNT_NUMBER</c>.</summary>
    AccountNumber = 2,

    /// <summary><c>MOOLRE_SANDBOX_PHONE</c>.</summary>
    PhoneNumber = 4,

    /// <summary><c>MOOLRE_SANDBOX_RECEIVER_ACCOUNT_NUMBER</c>.</summary>
    ReceiverAccountNumber = 8,

    /// <summary><c>MOOLRE_SANDBOX_SMS_VASKEY</c>.</summary>
    SmsKey = 16,

    /// <summary><c>MOOLRE_SANDBOX_SMS_SENDERID</c>.</summary>
    SmsSenderId = 32,
}
