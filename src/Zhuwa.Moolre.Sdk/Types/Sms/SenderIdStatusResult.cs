namespace Zhuwa.Moolre;

/// <summary>Result of the sender ID status operation.</summary>
public class SenderIdStatusResult : MoolreResult
{
    internal SenderIdStatusResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string senderId,
        SenderIdApproval approval,
        bool isWhitelisted)
        : base(code, message, messages, httpStatusCode)
    {
        SenderId = senderId;
        Approval = approval;
        IsWhitelisted = isWhitelisted;
    }

    /// <summary>Sender ID. Moolre field <c>data.senderid</c>.</summary>
    public string SenderId { get; }

    /// <summary>Approval. Moolre field <c>data.approval</c>.</summary>
    public SenderIdApproval Approval { get; }

    /// <summary>Is whitelisted. Moolre field <c>data.whitelisted</c>.</summary>
    public bool IsWhitelisted { get; }
}
