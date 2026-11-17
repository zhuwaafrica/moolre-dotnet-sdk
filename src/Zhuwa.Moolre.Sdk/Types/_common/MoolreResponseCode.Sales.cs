namespace Zhuwa.Moolre;

// Airtime, data bundle and sale status codes.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>Bundle catalog (<c>ASB02</c>).</summary>
    public static MoolreResponseCode BundleCatalog { get; } = new("ASB02");

    /// <summary>Sale authentication error (<c>ASL00</c>).</summary>
    public static MoolreResponseCode SaleAuthenticationError { get; } = new("ASL00");

    /// <summary>Sale amount invalid (<c>ASL01</c>).</summary>
    public static MoolreResponseCode SaleAmountInvalid { get; } = new("ASL01");

    /// <summary>Sale receiver missing (<c>ASL02</c>).</summary>
    public static MoolreResponseCode SaleReceiverMissing { get; } = new("ASL02");

    /// <summary>Sale incomplete (<c>ASL03</c>).</summary>
    public static MoolreResponseCode SaleIncomplete { get; } = new("ASL03");

    /// <summary>Sale channel not supported (<c>ASL04</c>).</summary>
    public static MoolreResponseCode SaleChannelNotSupported { get; } = new("ASL04");

    /// <summary>Sale type invalid (<c>ASL05</c>).</summary>
    public static MoolreResponseCode SaleTypeInvalid { get; } = new("ASL05");

    /// <summary>Sale outcome unknown (<c>ASL06</c>).</summary>
    public static MoolreResponseCode SaleOutcomeUnknown { get; } = new("ASL06");

    /// <summary>Sale duplicate reference (<c>ASL08</c>).</summary>
    public static MoolreResponseCode SaleDuplicateReference { get; } = new("ASL08");

    /// <summary>Sale currency not ghs (<c>ASL09</c>).</summary>
    public static MoolreResponseCode SaleCurrencyNotGhs { get; } = new("ASL09");

    /// <summary>Sale wallet currency not supported (<c>ASL10</c>).</summary>
    public static MoolreResponseCode SaleWalletCurrencyNotSupported { get; } = new("ASL10");

    /// <summary>Sale in progress (<c>ASL11</c>).</summary>
    public static MoolreResponseCode SaleInProgress { get; } = new("ASL11");

    /// <summary>Sale wallet inactive (<c>ASL12</c>).</summary>
    public static MoolreResponseCode SaleWalletInactive { get; } = new("ASL12");

    /// <summary>Sale account not found (<c>ASL13</c>).</summary>
    public static MoolreResponseCode SaleAccountNotFound { get; } = new("ASL13");

    /// <summary>Sale bundle missing (<c>ASL14</c>).</summary>
    public static MoolreResponseCode SaleBundleMissing { get; } = new("ASL14");

    /// <summary>Sale bundle not on network (<c>ASL15</c>).</summary>
    public static MoolreResponseCode SaleBundleNotOnNetwork { get; } = new("ASL15");

    /// <summary>Sale status authentication error (<c>ASS00</c>).</summary>
    public static MoolreResponseCode SaleStatusAuthenticationError { get; } = new("ASS00");

    /// <summary>Sale sent (<c>ASS01</c>).</summary>
    public static MoolreResponseCode SaleSent { get; } = new("ASS01");

    /// <summary>Sale failed and refunded (<c>ASS02</c>).</summary>
    public static MoolreResponseCode SaleFailedAndRefunded { get; } = new("ASS02");

    /// <summary>Sale pending (<c>ASS03</c>).</summary>
    public static MoolreResponseCode SalePending { get; } = new("ASS03");

    /// <summary>Sale not found (<c>ASS04</c>).</summary>
    public static MoolreResponseCode SaleNotFound { get; } = new("ASS04");

    /// <summary>Sale ID type invalid (<c>ASS05</c>).</summary>
    public static MoolreResponseCode SaleIdTypeInvalid { get; } = new("ASS05");

    /// <summary>Sale status incomplete (<c>ASS06</c>).</summary>
    public static MoolreResponseCode SaleStatusIncomplete { get; } = new("ASS06");

    /// <summary>Sale status type invalid (<c>ASS07</c>).</summary>
    public static MoolreResponseCode SaleStatusTypeInvalid { get; } = new("ASS07");

    /// <summary>Sale status unknown (<c>ASS08</c>).</summary>
    public static MoolreResponseCode SaleStatusUnknown { get; } = new("ASS08");
}
