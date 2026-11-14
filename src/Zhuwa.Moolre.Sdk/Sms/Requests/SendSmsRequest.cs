using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the send SMS operation.</summary>
public class SendSmsRequest
{
    /// <summary>Sender ID. Moolre field <c>senderid</c> (max 11 characters, must be approved).</summary>
    public required string SenderId { get; init; }

    /// <summary>Network. Moolre field <c>network</c> (live only).</summary>
    public SmsDeliveryNetwork Network { get; init; }

    /// <summary>Messages. Moolre field <c>messages</c> (initialised empty).</summary>
    public Collection<SmsMessage> Messages { get; } = [];
}
