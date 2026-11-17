namespace Zhuwa.Moolre;

// Wallet and account codes: create, update, status and transactions.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Wallet created (<c>WC02</c>).</summary>
    public static MoolreResponseCode WalletCreated { get; } = new("WC02");

    /// <summary>Wallet created alt (<c>APGW00</c>).</summary>
    public static MoolreResponseCode WalletCreatedAlt { get; } = new("APGW00");

    /// <summary>Account updated (<c>WCU02</c>).</summary>
    public static MoolreResponseCode AccountUpdated { get; } = new("WCU02");

    /// <summary>Wallet updated (<c>APGW16</c>).</summary>
    public static MoolreResponseCode WalletUpdated { get; } = new("APGW16");

    /// <summary>Update invalid account number (<c>WCU01</c>).</summary>
    public static MoolreResponseCode UpdateInvalidAccountNumber { get; } = new("WCU01");

    /// <summary>Currency not supported for API (<c>APGW06</c>).</summary>
    public static MoolreResponseCode CurrencyNotSupportedForApi { get; } = new("APGW06");

    /// <summary>Wallet not found on update (<c>APGW12</c>).</summary>
    public static MoolreResponseCode WalletNotFoundOnUpdate { get; } = new("APGW12");

    /// <summary>Duplicate account name (<c>APGW21</c>).</summary>
    public static MoolreResponseCode DuplicateAccountName { get; } = new("APGW21");

    /// <summary>Wallet found (<c>SW01</c>).</summary>
    public static MoolreResponseCode WalletFound { get; } = new("SW01");

    /// <summary>Transactions found (<c>ST08</c>).</summary>
    public static MoolreResponseCode TransactionsFound { get; } = new("ST08");

    /// <summary>Transactions not found (<c>ST09</c>).</summary>
    public static MoolreResponseCode TransactionsNotFound { get; } = new("ST09");
}
