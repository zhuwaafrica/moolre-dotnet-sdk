namespace Zhuwa.Moolre;

/// <summary>
/// A call that moves money or sends a message timed out or failed after the request may have reached Moolre.
/// The SDK never retries these calls. Look up the result with the status call and your <see cref="ExternalReference"/>
/// before deciding anything; do not resend with a new reference.
/// </summary>
public class MoolreOutcomeUncertainException : MoolreException
{
    /// <summary>Creates the exception.</summary>
    public MoolreOutcomeUncertainException()
        : this("The outcome of the Moolre call is unknown.", operation: string.Empty, externalReference: null)
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What happened.</param>
    public MoolreOutcomeUncertainException(string message)
        : this(message, operation: string.Empty, externalReference: null)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What happened.</param>
    /// <param name="innerException">The cause.</param>
    public MoolreOutcomeUncertainException(string message, Exception innerException)
        : base(message, innerException)
    {
        Operation = string.Empty;
    }

    /// <summary>Creates the exception for one operation.</summary>
    /// <param name="message">What happened.</param>
    /// <param name="operation">The SDK operation, for example <c>Transfers.SendAsync</c>.</param>
    /// <param name="externalReference">The caller's reference for the call, if the request had one.</param>
    /// <param name="innerException">The cause, if any.</param>
    public MoolreOutcomeUncertainException(string message, string operation, string? externalReference, Exception? innerException = null)
        : base(message, innerException!)
    {
        Operation = operation;
        ExternalReference = externalReference;
    }

    /// <summary>The caller's reference for the call. Use it to look up the real outcome.</summary>
    public string? ExternalReference { get; }

    /// <summary>The SDK operation, for example <c>Transfers.SendAsync</c>.</summary>
    public string Operation { get; }
}
