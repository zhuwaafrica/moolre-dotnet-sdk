namespace Zhuwa.Moolre;

/// <summary>The base of every exception the SDK throws for a problem with a Moolre call.</summary>
public class MoolreException : Exception
{
    /// <summary>Creates the exception.</summary>
    public MoolreException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What went wrong.</param>
    public MoolreException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
