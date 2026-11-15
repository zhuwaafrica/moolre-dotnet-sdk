using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Zhuwa.Moolre.Webhooks;

/// <summary>Checks callback secrets in constant time.</summary>
internal sealed class DefaultMoolreWebhookVerifier(IOptionsMonitor<MoolreWebhookOptions> options) : MoolreWebhookVerifier
{
    public override bool IsSecretValid(PaymentWebhook webhook)
    {
        ArgumentNullException.ThrowIfNull(webhook);

        var expected = options.CurrentValue.Secret;
        if (string.IsNullOrEmpty(expected))
        {
            throw new InvalidOperationException("Set MoolreWebhookOptions.Secret (with AddMoolreWebhooks) before verifying callbacks.");
        }

        var received = webhook.Data.Secret;
        if (string.IsNullOrEmpty(received))
        {
            return false;
        }

        // Both values are hashed first so the comparison always covers 32 bytes: FixedTimeEquals then takes the same time whatever
        // the input, and does not even reveal whether the lengths differ.
        Span<byte> expectedHash = stackalloc byte[SHA256.HashSizeInBytes];
        Span<byte> receivedHash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(Encoding.UTF8.GetBytes(expected), expectedHash);
        SHA256.HashData(Encoding.UTF8.GetBytes(received), receivedHash);

        return CryptographicOperations.FixedTimeEquals(expectedHash, receivedHash);
    }
}
