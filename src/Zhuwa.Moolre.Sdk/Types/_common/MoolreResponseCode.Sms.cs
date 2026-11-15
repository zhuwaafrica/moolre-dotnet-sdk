namespace Zhuwa.Moolre;

// SMS sending, balance and sender ID codes.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>SMS sent (<c>SMS01</c>).</summary>
    public static MoolreResponseCode SmsSent { get; } = new("SMS01");

    /// <summary>Sender ID not approved (<c>ASMS07</c>).</summary>
    public static MoolreResponseCode SenderIdNotApproved { get; } = new("ASMS07");

    /// <summary>Sender ID request unknown (<c>ASMS09</c>).</summary>
    public static MoolreResponseCode SenderIdRequestUnknown { get; } = new("ASMS09");

    /// <summary>Sender ID status (<c>ASMQ01</c>).</summary>
    public static MoolreResponseCode SenderIdStatus { get; } = new("ASMQ01");

    /// <summary>SMS balance (<c>ASMQ03</c>).</summary>
    public static MoolreResponseCode SmsBalance { get; } = new("ASMQ03");

    /// <summary>Sender ID required (<c>ASMQ06</c>).</summary>
    public static MoolreResponseCode SenderIdRequired { get; } = new("ASMQ06");

    /// <summary>Sender ids updated (<c>ASMQ07</c>).</summary>
    public static MoolreResponseCode SenderIdsUpdated { get; } = new("ASMQ07");

    /// <summary>Sender ID list (<c>ASMQ08</c>).</summary>
    public static MoolreResponseCode SenderIdList { get; } = new("ASMQ08");

    /// <summary>Sender ID no permission (<c>ASMQ09</c>).</summary>
    public static MoolreResponseCode SenderIdNoPermission { get; } = new("ASMQ09");

    /// <summary>SMS status (<c>ASMQ10</c>).</summary>
    public static MoolreResponseCode SmsStatus { get; } = new("ASMQ10");

    /// <summary>Sender ids created (<c>ASMQ12</c>).</summary>
    public static MoolreResponseCode SenderIdsCreated { get; } = new("ASMQ12");
}
