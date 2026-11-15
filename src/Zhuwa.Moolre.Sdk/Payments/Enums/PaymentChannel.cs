namespace Zhuwa.Moolre.Payments;

/// <summary>Payment channel.</summary>
public enum PaymentChannel
{
    /// <summary>None.</summary>
    None = 0,

    /// <summary>MTN. Moolre value <c>13</c>.</summary>
    Mtn = 13,

    /// <summary>Telecel. Moolre value <c>6</c>.</summary>
    Telecel = 6,

    /// <summary>AT. Moolre value <c>7</c>.</summary>
    At = 7,
}
