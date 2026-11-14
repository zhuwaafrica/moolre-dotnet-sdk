namespace Zhuwa.Moolre.Webhooks;

/// <summary>Settings for receiving Moolre webhooks.</summary>
public class MoolreWebhookOptions
{
    /// <summary>Secret. the expected callback secret for the wallet.</summary>
    public string? Secret { get; set; }
}
