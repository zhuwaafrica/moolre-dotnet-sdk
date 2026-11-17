namespace Zhuwa.Moolre;

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

    /// <summary>The name on the account, for example <c>BRIGHT BUAME</c>; empty when the account was not found. Moolre field <c>data</c>.</summary>
    public string AccountHolderName { get; }

    /// <summary>Whether Moolre found the account (<c>AVD01</c>). <see langword="false"/> for an unknown number (<c>AVD02</c>).</summary>
    public bool Found => Code == MoolreResponseCode.NameValidated;
}
