namespace Zhuwa.Moolre.Sms;

/// <summary>Sender ID registration.</summary>
public class SenderIdRegistration
{
    /// <summary>Sender ID. Moolre field <c>senderid</c> (max 11 characters).</summary>
    public required string SenderId { get; init; }

    /// <summary>Approve. Moolre field <c>approve</c> (needs permission).</summary>
    public bool? Approve { get; init; }
}
