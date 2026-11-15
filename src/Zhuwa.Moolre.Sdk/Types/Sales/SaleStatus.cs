namespace Zhuwa.Moolre;

/// <summary>Sale status.</summary>
public enum SaleStatus
{
    /// <summary>Pending.</summary>
    Pending = 0,

    /// <summary>Sent.</summary>
    Sent = 1,

    /// <summary>Failed and refunded.</summary>
    FailedAndRefunded = 2,

    /// <summary>Not found.</summary>
    NotFound = 3,
}
