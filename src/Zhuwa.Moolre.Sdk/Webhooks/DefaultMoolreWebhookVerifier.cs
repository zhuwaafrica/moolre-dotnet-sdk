using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Zhuwa.Moolre.Webhooks;

/// <summary>
/// Checks callback signatures (and the deprecated callback secret) in constant time. The signature rules follow Moolre's own PHP SDK
/// (<c>Moolre\Webhook::check</c>) and its published test vectors.
/// </summary>
internal sealed class DefaultMoolreWebhookVerifier(IOptionsMonitor<MoolreWebhookOptions> options, TimeProvider timeProvider) : MoolreWebhookVerifier
{
    /// <summary>A <c>v1</c> signature is a hex-encoded HMAC-SHA256: 32 bytes, 64 characters.</summary>
    private const int SignatureHexLength = HMACSHA256.HashSizeInBytes * 2;

    /// <summary>Moolre's limit on the timestamp's digits; it also keeps the value far inside <see cref="long"/>.</summary>
    private const int MaxTimestampDigits = 15;

    public override bool IsSignatureValid(ReadOnlySpan<byte> rawBody, string? signatureHeader)
    {
        var settings = CurrentSettings();

        if (!TryReadSignatureHeader(signatureHeader, out var timestamp, out var signatures))
        {
            return false;
        }

        // The wallet is read from the body before it is trusted. That is safe: a forged account number only selects which secret the
        // signature must match, and the sender still cannot produce a valid signature without that secret. Bodies that are not payment
        // callbacks (for example webhook.test) name no wallet and use the fallback secret.
        var accountNumber = PaymentWebhook.TryParse(rawBody, out var webhook) ? webhook.Data.AccountNumber : string.Empty;
        var secret = SecretFor(accountNumber, settings);
        if (string.IsNullOrEmpty(secret))
        {
            return false;
        }

        // Moolre signs "<t>.<raw body>", with t exactly as it appears in the header.
        Span<byte> expected = stackalloc byte[HMACSHA256.HashSizeInBytes];
        using (var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(secret)))
        {
            hmac.AppendData(Encoding.ASCII.GetBytes(timestamp));
            hmac.AppendData("."u8);
            hmac.AppendData(rawBody);
            hmac.GetHashAndReset(expected);
        }

        // Every candidate is compared, without stopping at the first match, so the time taken does not reveal which one matched.
        var matched = false;
        foreach (var signature in signatures)
        {
            matched |= CryptographicOperations.FixedTimeEquals(expected, signature);
        }

        if (!matched)
        {
            return false;
        }

        // Checked after the signature, so a forged request learns nothing about the clock. Both directions count: a timestamp from the
        // future is as suspicious as an old one.
        var age = Math.Abs(timeProvider.GetUtcNow().ToUnixTimeSeconds() - long.Parse(timestamp, CultureInfo.InvariantCulture));
        return age <= (long)settings.SignatureTolerance.TotalSeconds;
    }

    [Obsolete(SecretFieldObsoleteMessage)]
    public override bool IsSecretValid(PaymentWebhook webhook)
    {
        ArgumentNullException.ThrowIfNull(webhook);

        var expected = SecretFor(webhook.Data.AccountNumber, CurrentSettings());
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

    /// <summary>The current options, refusing to run without any secret.</summary>
    /// <exception cref="InvalidOperationException">No secret is configured.</exception>
    private MoolreWebhookOptions CurrentSettings()
    {
        var settings = options.CurrentValue;
        if (string.IsNullOrEmpty(settings.Secret) && settings.WalletSecrets.Count == 0)
        {
            throw new InvalidOperationException(
                "Set MoolreWebhookOptions.Secret or MoolreWebhookOptions.WalletSecrets (with AddMoolreWebhooks) before verifying callbacks.");
        }

        return settings;
    }

    /// <summary>
    /// Moolre gives each wallet its own secret, so a callback is checked against the secret of the wallet it names. A wallet with no secret of
    /// its own falls back to <see cref="MoolreWebhookOptions.Secret"/>; with no fallback either, the result is null and the callback is rejected.
    /// </summary>
    private static string? SecretFor(string accountNumber, MoolreWebhookOptions settings) =>
        settings.WalletSecrets.TryGetValue(accountNumber.Trim(), out var walletSecret) ? walletSecret : settings.Secret;

    /// <summary>
    /// Reads <c>t=&lt;digits&gt;,v1=&lt;hex&gt;[,v1=&lt;hex&gt;...]</c>. Spaces around parts are allowed, the first <c>t</c> wins, every
    /// well-formed <c>v1</c> is kept (Moolre may send more than one while rotating secrets) and other schemes are ignored.
    /// </summary>
    /// <returns><see langword="false"/> when the header is missing, has no valid timestamp, or has no well-formed <c>v1</c>.</returns>
    private static bool TryReadSignatureHeader(string? header, out string timestamp, out List<byte[]> signatures)
    {
        timestamp = string.Empty;
        signatures = [];
        string? foundTimestamp = null;

        if (string.IsNullOrEmpty(header))
        {
            return false;
        }

        foreach (var part in header.Split(','))
        {
            var separator = part.IndexOf('=', StringComparison.Ordinal);
            if (separator < 0)
            {
                continue;
            }

            var key = part.AsSpan(0, separator).Trim();
            var value = part.AsSpan(separator + 1).Trim();

            if (key.SequenceEqual("t") && foundTimestamp is null)
            {
                foundTimestamp = value.ToString();
            }
            else if (key.SequenceEqual("v1") && value.Length == SignatureHexLength && IsHex(value))
            {
                signatures.Add(Convert.FromHexString(value));
            }
        }

        if (foundTimestamp is not { Length: > 0 and <= MaxTimestampDigits } || !foundTimestamp.All(char.IsAsciiDigit) || signatures.Count == 0)
        {
            return false;
        }

        timestamp = foundTimestamp;
        return true;
    }

    private static bool IsHex(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (!char.IsAsciiHexDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
