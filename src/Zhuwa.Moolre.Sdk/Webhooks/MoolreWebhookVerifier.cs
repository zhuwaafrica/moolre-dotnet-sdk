namespace Zhuwa.Moolre.Webhooks;

/// <summary>
/// Checks that a payment callback really came from Moolre before you act on it. Register it with <c>AddMoolreWebhooks</c> and inject it
/// into your webhook endpoint. Moolre documents no signature, so the callback secret is the verification. Moolre also publishes the
/// addresses it sends callbacks from; filter those in your firewall or reverse proxy (see the README), not in application code.
/// </summary>
public abstract class MoolreWebhookVerifier
{
    /// <summary>Creates the verifier. For subclasses and test doubles.</summary>
    protected MoolreWebhookVerifier()
    {
    }

    /// <summary>
    /// Whether the callback carries the secret you configured in <see cref="MoolreWebhookOptions.Secret"/>. The comparison takes the
    /// same time whatever the input, so timing cannot reveal the secret. Reject the callback when this is <see langword="false"/>.
    /// </summary>
    /// <param name="webhook">The parsed callback.</param>
    /// <returns><see langword="true"/> only when the secrets match exactly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="webhook"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No secret is configured; a missing setting never counts as a match.</exception>
    public abstract bool IsSecretValid(PaymentWebhook webhook);
}
