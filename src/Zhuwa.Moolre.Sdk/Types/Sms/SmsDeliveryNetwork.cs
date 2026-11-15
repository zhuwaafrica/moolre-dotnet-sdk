namespace Zhuwa.Moolre;

/// <summary>The network Moolre delivers an SMS over (not the recipient's network). Moolre accepts this in live only.</summary>
public enum SmsDeliveryNetwork
{
    /// <summary>Not sent; Moolre delivers over Telecel.</summary>
    Default = 0,

    /// <summary>Telecel. Moolre value <c>TELECEL</c>.</summary>
    Telecel = 1,

    /// <summary>MTN. Moolre value <c>MTN</c>. Used only when Moolre has approved MTN delivery for you; otherwise Moolre falls back to Telecel.</summary>
    Mtn = 2,
}
