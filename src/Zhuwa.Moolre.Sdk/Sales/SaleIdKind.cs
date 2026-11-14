namespace Zhuwa.Moolre.Sales;

/// <summary>Sale ID kind.</summary>
public enum SaleIdKind
{
    /// <summary>None.</summary>
    None = 0,

    /// <summary>External reference. wire idtype 1 (original externalref / extid)</summary>
    ExternalReference = 1,

    /// <summary>Transaction ID. wire idtype 2</summary>
    TransactionId = 2,

    /// <summary>Sale reference. wire idtype 3 (the reference returned by the sale)</summary>
    SaleReference = 3,
}
