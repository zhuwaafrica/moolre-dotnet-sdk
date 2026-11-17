using System.Text;

namespace Zhuwa.Moolre.Webhooks;

/// <summary>
/// Checks that a callback really came from Moolre before you act on it. Register it with <c>AddMoolreWebhooks</c> and inject it into your
/// webhook endpoint. Moolre signs every callback with the wallet's callback secret and sends the signature in the
/// <see cref="SignatureHeaderName"/> header. Moolre also publishes the addresses it sends callbacks from; filter those in your firewall or
/// reverse proxy (see the README), not in application code.
/// </summary>
public abstract class MoolreWebhookVerifier
{
    /// <summary>The request header that carries Moolre's callback signature: <c>t=&lt;unix seconds&gt;,v1=&lt;hex HMAC-SHA256&gt;</c>.</summary>
    public const string SignatureHeaderName = "X-Moolre-Signature";

    /// <summary>Shared by every member that reads the deprecated <c>data.secret</c> field, so the guidance is written once.</summary>
    internal const string SecretFieldObsoleteMessage =
        "Moolre deprecated data.secret and removes it after 31 March 2027. Verify the X-Moolre-Signature header with IsSignatureValid instead.";

    /// <summary>Creates the verifier. For subclasses and test doubles.</summary>
    protected MoolreWebhookVerifier()
    {
    }

    /// <summary>
    /// Whether the callback carries a valid Moolre signature: the <see cref="SignatureHeaderName"/> header must hold a <c>v1</c> HMAC-SHA256 of
    /// <c>"&lt;t&gt;.&lt;raw body&gt;"</c> keyed with the callback's wallet secret (from <see cref="MoolreWebhookOptions.WalletSecrets"/> for the
    /// callback's account number, or <see cref="MoolreWebhookOptions.Secret"/> otherwise), and <c>t</c> must be within
    /// <see cref="MoolreWebhookOptions.SignatureTolerance"/> of the current time. The comparison takes the same time whatever the input. Reject
    /// the callback when this is <see langword="false"/>.
    /// </summary>
    /// <param name="rawBody">The request body exactly as received. Re-serialised JSON gives different bytes and never matches.</param>
    /// <param name="signatureHeader">The value of the <see cref="SignatureHeaderName"/> header; <see langword="null"/> when it was absent.</param>
    /// <returns><see langword="true"/> only when a signature matches and its timestamp is recent.</returns>
    /// <exception cref="InvalidOperationException">No secret is configured at all; a missing setting never counts as a match.</exception>
    public virtual bool IsSignatureValid(ReadOnlySpan<byte> rawBody, string? signatureHeader) =>
        throw new NotImplementedException();

    /// <summary>
    /// Whether the callback carries a valid Moolre signature, with the body given as text. See
    /// <see cref="IsSignatureValid(ReadOnlySpan{byte}, string)"/>; prefer that overload with the raw bytes when you have them.
    /// </summary>
    /// <param name="rawBody">The request body exactly as received.</param>
    /// <param name="signatureHeader">The value of the <see cref="SignatureHeaderName"/> header; <see langword="null"/> when it was absent.</param>
    /// <returns><see langword="true"/> only when a signature matches and its timestamp is recent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rawBody"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No secret is configured at all.</exception>
    public bool IsSignatureValid(string rawBody, string? signatureHeader)
    {
        ArgumentNullException.ThrowIfNull(rawBody);

        // Moolre signs the UTF-8 bytes it sent, and decoding then re-encoding valid UTF-8 gives those same bytes back.
        return IsSignatureValid(Encoding.UTF8.GetBytes(rawBody), signatureHeader);
    }

    /// <summary>
    /// Whether the callback carries the right secret for its wallet in its deprecated <c>data.secret</c> field: the one in
    /// <see cref="MoolreWebhookOptions.WalletSecrets"/> for the callback's account number, or <see cref="MoolreWebhookOptions.Secret"/>
    /// otherwise. A callback for a wallet with no secret configured is rejected. The comparison takes the same time whatever the input.
    /// </summary>
    /// <param name="webhook">The parsed callback.</param>
    /// <returns><see langword="true"/> only when the secrets match exactly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="webhook"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No secret is configured at all; a missing setting never counts as a match.</exception>
    [Obsolete(SecretFieldObsoleteMessage)]
    public abstract bool IsSecretValid(PaymentWebhook webhook);
}
