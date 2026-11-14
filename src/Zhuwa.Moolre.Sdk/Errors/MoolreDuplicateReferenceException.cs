using System.Net;

namespace Zhuwa.Moolre;

/// <summary>The external reference was already used (TP13, TR013, APY18, ASL08, INP02). Do not resend; check the status of the original call.</summary>
public class MoolreDuplicateReferenceException : MoolreApiException
{
    /// <summary>Creates the exception.</summary>
    public MoolreDuplicateReferenceException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">Moolre's message.</param>
    public MoolreDuplicateReferenceException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreDuplicateReferenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">Moolre's message.</param>
    /// <param name="code">Moolre's application code.</param>
    /// <param name="httpStatusCode">The HTTP status code of the response.</param>
    public MoolreDuplicateReferenceException(string message, MoolreResponseCode code, HttpStatusCode httpStatusCode)
        : base(message, code, httpStatusCode)
    {
    }

    internal MoolreDuplicateReferenceException(
        string message,
        MoolreResponseCode code,
        HttpStatusCode httpStatusCode,
        IReadOnlyList<string> messages,
        string? detail)
        : base(message, code, httpStatusCode, messages, detail)
    {
    }
}
