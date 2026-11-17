namespace Zhuwa.Moolre;

// Collection, payment status, Payment ID, bank account and payment link codes.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Transaction successful (<c>SS01</c>).</summary>
    public static MoolreResponseCode TransactionSuccessful { get; } = new("SS01");

    /// <summary>Currency invalid (<c>TP01</c>).</summary>
    public static MoolreResponseCode CurrencyInvalid { get; } = new("TP01");

    /// <summary>Channel not supported (<c>TP09</c>).</summary>
    public static MoolreResponseCode ChannelNotSupported { get; } = new("TP09");

    /// <summary>Collection duplicate reference (<c>TP13</c>).</summary>
    public static MoolreResponseCode CollectionDuplicateReference { get; } = new("TP13");

    /// <summary>OTP required (<c>TP14</c>).</summary>
    public static MoolreResponseCode OtpRequired { get; } = new("TP14");

    /// <summary>Phone verified (<c>TP17</c>).</summary>
    public static MoolreResponseCode PhoneVerified { get; } = new("TP17");

    /// <summary>Payment succeeded (<c>P01</c>).</summary>
    public static MoolreResponseCode PaymentSucceeded { get; } = new("P01");

    /// <summary>Payment failed (<c>P02</c>).</summary>
    public static MoolreResponseCode PaymentFailed { get; } = new("P02");

    /// <summary>Payment ID created (<c>AD14</c>).</summary>
    public static MoolreResponseCode PaymentIdCreated { get; } = new("AD14");

    /// <summary>Bank account created (<c>AD19</c>).</summary>
    public static MoolreResponseCode BankAccountCreated { get; } = new("AD19");

    /// <summary>Bank account creation failed (<c>AD32</c>).</summary>
    public static MoolreResponseCode BankAccountCreationFailed { get; } = new("AD32");

    /// <summary>Payment account found (<c>AC01</c>).</summary>
    public static MoolreResponseCode PaymentAccountFound { get; } = new("AC01");

    /// <summary>Payment account not found (<c>AC03</c>).</summary>
    public static MoolreResponseCode PaymentAccountNotFound { get; } = new("AC03");

    /// <summary>Payment link created (<c>POS09</c>).</summary>
    public static MoolreResponseCode PaymentLinkCreated { get; } = new("POS09");

    /// <summary>Payment link duplicate (<c>INP02</c>).</summary>
    public static MoolreResponseCode PaymentLinkDuplicate { get; } = new("INP02");
}
