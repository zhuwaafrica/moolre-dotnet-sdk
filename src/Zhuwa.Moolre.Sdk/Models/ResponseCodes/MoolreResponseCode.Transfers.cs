namespace Zhuwa.Moolre;

// Name validation, payout and internal transfer codes.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Name validated (<c>AVD01</c>).</summary>
    public static MoolreResponseCode NameValidated { get; } = new("AVD01");

    /// <summary>Name not found (<c>AVD02</c>).</summary>
    public static MoolreResponseCode NameNotFound { get; } = new("AVD02");

    /// <summary>Payout successful (<c>OBGH01</c>).</summary>
    public static MoolreResponseCode PayoutSuccessful { get; } = new("OBGH01");

    /// <summary>Payout failed (<c>APY17</c>).</summary>
    public static MoolreResponseCode PayoutFailed { get; } = new("APY17");

    /// <summary>Payout duplicate reference (<c>APY18</c>).</summary>
    public static MoolreResponseCode PayoutDuplicateReference { get; } = new("APY18");

    /// <summary>Insufficient wallet balance (<c>APY21</c>).</summary>
    public static MoolreResponseCode InsufficientWalletBalance { get; } = new("APY21");

    /// <summary>Payout in progress (<c>OTGH02</c>).</summary>
    public static MoolreResponseCode PayoutInProgress { get; } = new("OTGH02");

    /// <summary>Transfer initiated (<c>TR099</c>).</summary>
    public static MoolreResponseCode TransferInitiated { get; } = new("TR099");

    /// <summary>Internal transfer duplicate reference (<c>TR013</c>).</summary>
    public static MoolreResponseCode InternalTransferDuplicateReference { get; } = new("TR013");

    /// <summary>Internal transfer insufficient balance (<c>TR09</c>).</summary>
    public static MoolreResponseCode InternalTransferInsufficientBalance { get; } = new("TR09");

    /// <summary>Internal transfer completed (<c>OM03</c>).</summary>
    public static MoolreResponseCode InternalTransferCompleted { get; } = new("OM03");
}
