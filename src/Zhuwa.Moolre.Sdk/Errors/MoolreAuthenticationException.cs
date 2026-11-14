using System.Net;

namespace Zhuwa.Moolre;

/// <summary>The credentials were rejected, the wallet was not found, or API access is not activated (AIN01, AIN03, AIN04, IN01, ASL00, ASS00).</summary>
public class MoolreAuthenticationException : MoolreApiException
{
    /// <summary>Creates the exception.</summary>
    public MoolreAuthenticationException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">Moolre's message.</param>
    public MoolreAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">Moolre's message.</param>
    /// <param name="code">Moolre's application code.</param>
    /// <param name="httpStatusCode">The HTTP status code of the response.</param>
    public MoolreAuthenticationException(string message, MoolreResponseCode code, HttpStatusCode httpStatusCode)
        : base(message, code, httpStatusCode)
    {
    }

    internal MoolreAuthenticationException(
        string message,
        MoolreResponseCode code,
        HttpStatusCode httpStatusCode,
        IReadOnlyList<string> messages,
        string? detail)
        : base(message, code, httpStatusCode, messages, detail)
    {
    }
}
