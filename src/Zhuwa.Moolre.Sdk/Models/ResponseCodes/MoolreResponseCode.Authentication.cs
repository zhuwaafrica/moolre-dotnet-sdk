namespace Zhuwa.Moolre;

// Authentication codes shared by several areas. The transport turns these into MoolreAuthenticationException.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Authentication error (<c>AIN01</c>).</summary>
    public static MoolreResponseCode AuthenticationError { get; } = new("AIN01");

    /// <summary>Wallet not found (<c>AIN03</c>).</summary>
    public static MoolreResponseCode WalletNotFound { get; } = new("AIN03");

    /// <summary>API access not activated (<c>AIN04</c>).</summary>
    public static MoolreResponseCode ApiAccessNotActivated { get; } = new("AIN04");

    /// <summary>Validate name authentication error (<c>IN01</c>).</summary>
    public static MoolreResponseCode ValidateNameAuthenticationError { get; } = new("IN01");
}
