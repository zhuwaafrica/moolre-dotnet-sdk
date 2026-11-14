namespace Zhuwa.Moolre.Reference;

/// <summary>Result of the bank list operation.</summary>
public class BankListResult : MoolreResult
{
    internal BankListResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<MoolreBank> banks)
        : base(code, message, messages, httpStatusCode)
    {
        Banks = banks;
    }

    /// <summary>Banks. Moolre field <c>data</c>.</summary>
    public IReadOnlyList<MoolreBank> Banks { get; }
}
