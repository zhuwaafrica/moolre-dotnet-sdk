namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the send single SMS operation.</summary>
public class SendSingleSmsRequest
{
    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    public required string SenderId { get; init; }

    /// <summary>Network. Moolre field <c>network</c>.</summary>
    public SmsDeliveryNetwork Network { get; init; }

    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    public required string Recipient { get; init; }

    /// <summary>Text. Moolre field <c>message</c> (max 160 characters).</summary>
    public required string Text { get; init; }
}
