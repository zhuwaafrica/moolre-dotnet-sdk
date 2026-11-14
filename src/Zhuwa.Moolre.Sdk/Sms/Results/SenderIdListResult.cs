namespace Zhuwa.Moolre.Sms;

/// <summary>Result of the sender ID list operation.</summary>
public class SenderIdListResult : MoolreResult
{
    internal SenderIdListResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<SenderIdInfo> senderIds)
        : base(code, message, messages, httpStatusCode)
    {
        SenderIds = senderIds;
    }

    /// <summary>Sender ids. Moolre field <c>data</c>.</summary>
    public IReadOnlyList<SenderIdInfo> SenderIds { get; }
}
