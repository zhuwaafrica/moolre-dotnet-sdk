namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the create sandbox account operation.</summary>
public class CreateSandboxAccountRequest
{
    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    public required string AccountName { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c> (GHS only).</summary>
    public Currency Currency { get; init; } = Currency.Ghs;

    /// <summary>API enabled. Moolre field <c>api</c>.</summary>
    public bool? ApiEnabled { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>Settlement. Moolre field <c>settlement</c>.</summary>
    public required SettlementSettings Settlement { get; init; }
}
