using System.Net;

namespace Zhuwa.Moolre;

/// <summary>Moolre answered the call with a failure (status <c>0</c> or a failure code).</summary>
public class MoolreApiException : MoolreException
{
    /// <summary>Creates the exception.</summary>
    public MoolreApiException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">Moolre's message.</param>
    public MoolreApiException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">Moolre's message.</param>
    /// <param name="code">Moolre's application code.</param>
    /// <param name="httpStatusCode">The HTTP status code of the response.</param>
    public MoolreApiException(string message, MoolreResponseCode code, HttpStatusCode httpStatusCode)
        : this(message, code, httpStatusCode, [], detail: null)
    {
    }

    internal MoolreApiException(
        string message,
        MoolreResponseCode code,
        HttpStatusCode httpStatusCode,
        IReadOnlyList<string> messages,
        string? detail)
        : base(message)
    {
        Code = code;
        HttpStatusCode = httpStatusCode;
        Messages = messages;
        Detail = detail;
    }

    /// <summary>Moolre's application code, the stable value to branch on. Moolre field <c>code</c>.</summary>
    public MoolreResponseCode Code { get; }

    /// <summary>The HTTP status code of the response.</summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>Moolre's messages, one entry per string; empty when Moolre sent none. Moolre field <c>message</c>.</summary>
    public IReadOnlyList<string> Messages { get; } = [];

    /// <summary>Extra text Moolre sent in <c>data</c> when it is a string (for example <c>senderid</c>), or <see langword="null"/>.</summary>
    public string? Detail { get; }
}
