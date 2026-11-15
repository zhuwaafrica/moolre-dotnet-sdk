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

        var settings = options.CurrentValue;
        if (string.IsNullOrEmpty(settings.Secret) && settings.WalletSecrets.Count == 0)
        {
            throw new InvalidOperationException(
                "Set MoolreWebhookOptions.Secret or MoolreWebhookOptions.WalletSecrets (with AddMoolreWebhooks) before verifying callbacks.");
        }

        // Moolre gives each wallet its own secret, so the callback is checked against the secret of the wallet it names. A wallet with
        // no secret of its own falls back to Secret; with no fallback either, the callback is rejected rather than accepted.
        var expected = settings.WalletSecrets.TryGetValue(webhook.Data.AccountNumber.Trim(), out var walletSecret) ? walletSecret : settings.Secret;
        var received = webhook.Data.Secret;
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(received))
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
