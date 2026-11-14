namespace Zhuwa.Moolre.Sms;

/// <summary>SMS delivery network.</summary>
public enum SmsDeliveryNetwork
{
    /// <summary>Default. not sent; Moolre delivers over TELECEL</summary>
    Default = 0,

    /// <summary>Telecel. Moolre value <c>TELECEL</c>.</summary>
    Telecel = 1,

    /// <summary>MTN. Moolre value <c>MTN</c>.</summary>
    Mtn = 2,
}
