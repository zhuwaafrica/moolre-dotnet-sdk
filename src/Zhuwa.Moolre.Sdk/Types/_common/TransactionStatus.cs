namespace Zhuwa.Moolre;

/// <summary>The state of a payment, transfer or sale as Moolre reports it.</summary>
public enum TransactionStatus
{
    /// <summary>Pending.</summary>
    Pending = 0,

    /// <summary>Successful.</summary>
    Successful = 1,

    /// <summary>Failed.</summary>
    Failed = 2,
}
