namespace Zhuwa.Moolre.Webhooks;

/// <summary>Settings for receiving Moolre webhooks.</summary>
public class MoolreWebhookOptions
{
    /// <summary>
    /// The callback secret for your wallet, which Moolre puts in every callback's <c>secret</c> field. Read it from configuration or a
    /// secret store, never source code. Required: callbacks cannot be verified without it.
    /// </summary>
    public string? Secret { get; set; }
}
