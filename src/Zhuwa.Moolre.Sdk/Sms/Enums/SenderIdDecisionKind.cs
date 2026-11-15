namespace Zhuwa.Moolre.Sms;

/// <summary>Sender ID decision kind.</summary>
public enum SenderIdDecisionKind
{
    /// <summary>Pending. wire: approve 0</summary>
    Pending = 0,

    /// <summary>Approved. wire: approve 1</summary>
    Approved = 1,

    /// <summary>Rejected. wire: approve 2</summary>
    Rejected = 2,
}
