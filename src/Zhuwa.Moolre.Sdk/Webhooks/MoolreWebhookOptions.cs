namespace Zhuwa.Moolre.Webhooks;

/// <summary>
/// The callback secrets the SDK verifies callbacks with. Moolre gives every wallet its own callback secret (returned as
/// <c>AccountDetails.Secret</c>) and signs each callback with it; each callback names its wallet in <c>data.accountnumber</c>. Set
/// <see cref="Secret"/> for one wallet, <see cref="WalletSecrets"/> for several, or both. Read secrets from configuration or a secret store,
/// never source code.
/// </summary>
public class MoolreWebhookOptions
{
    /// <summary>
    /// The callback secret for a single wallet, or the fallback for callbacks whose wallet is not in <see cref="WalletSecrets"/>.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// One callback secret per wallet, keyed by the wallet's account number (the callback's <c>data.accountnumber</c>). A callback is checked
    /// against its own wallet's secret, so one endpoint can receive callbacks for several wallets.
    /// </summary>
    public IDictionary<string, string> WalletSecrets { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// How far the signature's timestamp may be from this machine's clock, in either direction. Older callbacks are rejected so a captured
    /// request cannot be replayed later. Defaults to 5 minutes, the limit Moolre documents; must be greater than zero.
    /// </summary>
    public TimeSpan SignatureTolerance { get; set; } = TimeSpan.FromMinutes(5);
}
