namespace Zhuwa.Moolre;

/// <summary>Settings for the Moolre SDK: the environment, the credentials for each Moolre service and the default wallet.</summary>
public class MoolreOptions
{
    /// <summary>Environment. Sandbox unless set.</summary>
    public MoolreEnvironment Environment { get; set; }

    /// <summary>API user. X-API-USER.</summary>
    public string? ApiUser { get; set; }

    /// <summary>Private key. X-API-KEY (transfers, sales, accounts).</summary>
    public string? PrivateKey { get; set; }

    /// <summary>Public key. X-API-PUBKEY (collections, links, Payment IDs).</summary>
    public string? PublicKey { get; set; }

    /// <summary>SMS VAS key. X-API-VASKEY for SMS.</summary>
    public string? SmsVasKey { get; set; }

    /// <summary>WhatsApp VAS key. X-API-VASKEY for WhatsApp.</summary>
    public string? WhatsAppVasKey { get; set; }

    /// <summary>Account number. default wallet for requests that omit AccountNumber.</summary>
    public string? AccountNumber { get; set; }

    /// <summary>Timeout.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Base address. tests only; must be explicit.</summary>
    public Uri? BaseAddress { get; set; }
}
