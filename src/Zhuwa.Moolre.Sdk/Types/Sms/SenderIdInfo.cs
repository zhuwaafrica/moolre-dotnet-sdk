namespace Zhuwa.Moolre;

/// <summary>Sender ID info.</summary>
public class SenderIdInfo
{
    internal SenderIdInfo(
        string id,
        string senderId,
        SenderIdApproval approval,
        bool isWhitelisted)
    {
        Id = id;
        SenderId = senderId;
        Approval = approval;
        IsWhitelisted = isWhitelisted;
    }

    /// <summary>ID. Moolre field <c>id</c> (integer in the list, string in the approve response).</summary>
    public string Id { get; }

    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    public string SenderId { get; }

    /// <summary>Approval. Moolre field <c>approval</c>.</summary>
    public SenderIdApproval Approval { get; }

    /// <summary>Is whitelisted. Moolre field <c>whitelisted</c>.</summary>
    public bool IsWhitelisted { get; }
}
