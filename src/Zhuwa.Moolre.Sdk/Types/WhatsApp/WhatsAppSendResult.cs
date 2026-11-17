namespace Zhuwa.Moolre;

/// <summary>Result of the WhatsApp send operation.</summary>
public class WhatsAppSendResult : MoolreResult
{
    internal WhatsAppSendResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string? warning,
        IReadOnlyList<WhatsAppRejectedMessage> notSent)
        : base(code, message, messages, httpStatusCode)
    {
        Warning = warning;
        NotSent = notSent;
    }

    /// <summary>Warning. Moolre field <c>error</c> (a string such as "Some ref are not unique…"; an empty array becomes null).</summary>
    public string? Warning { get; }

    /// <summary>Not sent. Moolre field <c>data</c> (messages not sent because of duplicate references).</summary>
    public IReadOnlyList<WhatsAppRejectedMessage> NotSent { get; }

    /// <summary>Whether every message was accepted (no warning and nothing left unsent).</summary>
    public bool AllSent => Warning is null && NotSent.Count == 0;
}
