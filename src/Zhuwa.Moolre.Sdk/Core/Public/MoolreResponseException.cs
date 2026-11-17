using System.Net;

namespace Zhuwa.Moolre;

/// <summary>The response body could not be understood (not JSON, not an object, or not a Moolre envelope).</summary>
public class MoolreResponseException : MoolreException
{
    /// <summary>Creates the exception.</summary>
    public MoolreResponseException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What was wrong with the response.</param>
    public MoolreResponseException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What was wrong with the response.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception with the HTTP status code of the response.</summary>
    /// <param name="message">What was wrong with the response.</param>
    /// <param name="httpStatusCode">The HTTP status code of the response.</param>
    /// <param name="innerException">The cause, if any.</param>
    public MoolreResponseException(string message, HttpStatusCode httpStatusCode, Exception? innerException = null)
        : base(message, innerException!)
    {
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>The HTTP status code of the response (zero when there was no response).</summary>
    public HttpStatusCode HttpStatusCode { get; }
}
