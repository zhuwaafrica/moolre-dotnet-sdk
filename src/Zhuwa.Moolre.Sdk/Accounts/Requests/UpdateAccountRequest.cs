namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the update account operation.</summary>
public class UpdateAccountRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    public string? AccountName { get; init; }

    /// <summary>API enabled. Moolre field <c>api</c> (send only to change API access).</summary>
    public bool? ApiEnabled { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    public Uri? CallbackUrl { get; init; }

    /// <summary>Settlement. Moolre field <c>settlement</c>.</summary>
    public SettlementSettings? Settlement { get; init; }
}
