namespace Zhuwa.Moolre.Transfers;

/// <summary>Result of the name validation operation.</summary>
public class NameValidationResult : MoolreResult
{
    internal NameValidationResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string accountHolderName)
        : base(code, message, messages, httpStatusCode)
    {
        AccountHolderName = accountHolderName;
    }

    /// <summary>Account holder name. Moolre field <c>data</c> (a string, e.g. "BRIGHT BUAME").</summary>
    public string AccountHolderName { get; }
}
