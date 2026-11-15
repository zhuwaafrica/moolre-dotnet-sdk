namespace Zhuwa.Moolre;

/// <summary>The base of every result returned by an SDK call. It carries what Moolre said about the call.</summary>
public abstract class MoolreResult
{
    internal MoolreResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode)
    {
        Code = code;
        Message = message;
        Messages = messages;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>Moolre's application code, the stable value to branch on. Moolre field <c>code</c>.</summary>
    public MoolreResponseCode Code { get; }

    /// <summary>Moolre's message (an array of messages is joined with a space), or <see langword="null"/>. Moolre field <c>message</c>.</summary>
    public string? Message { get; }

    /// <summary>Moolre's messages, one entry per string; empty when Moolre sent none. Moolre field <c>message</c>.</summary>
    public IReadOnlyList<string> Messages { get; }

    /// <summary>The HTTP status code of the response.</summary>
    public System.Net.HttpStatusCode HttpStatusCode { get; }
}
