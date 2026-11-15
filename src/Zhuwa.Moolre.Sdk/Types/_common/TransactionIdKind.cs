namespace Zhuwa.Moolre;

/// <summary>Which identifier a status lookup uses.</summary>
public enum TransactionIdKind
{
    /// <summary>None.</summary>
    None = 0,

    /// <summary>External reference. wire idtype 1</summary>
    ExternalReference = 1,

    /// <summary>Moolre ID. wire idtype 2</summary>
    MoolreId = 2,
}
