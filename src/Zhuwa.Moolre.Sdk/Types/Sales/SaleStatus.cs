namespace Zhuwa.Moolre;

/// <summary>Where an airtime or data sale stands, from Moolre's sale status codes (<c>ASS01</c> to <c>ASS04</c>).</summary>
public enum SaleStatus
{
    /// <summary>Still being processed (<c>ASS03</c>). Check again later.</summary>
    Pending = 0,

    /// <summary>Delivered to the receiver (<c>ASS01</c>).</summary>
    Sent = 1,

    /// <summary>Failed, and the wallet was refunded (<c>ASS02</c>).</summary>
    FailedAndRefunded = 2,

    /// <summary>No sale with that identifier (<c>ASS04</c>).</summary>
    NotFound = 3,
}
