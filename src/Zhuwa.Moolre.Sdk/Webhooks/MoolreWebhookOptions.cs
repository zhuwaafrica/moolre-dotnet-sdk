namespace Zhuwa.Moolre.Webhooks;

/// <summary>
/// The callback secrets the SDK checks payment callbacks against. Moolre gives every wallet its own callback secret (returned as
/// <c>AccountDetails.Secret</c>), and each callback names its wallet in <c>data.accountnumber</c>. Set <see cref="Secret"/> for one wallet,
/// <see cref="WalletSecrets"/> for several, or both. Read secrets from configuration or a secret store, never source code.
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
}
