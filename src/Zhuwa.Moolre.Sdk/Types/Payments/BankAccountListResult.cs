namespace Zhuwa.Moolre;

/// <summary>Result of the bank account list operation.</summary>
public class BankAccountListResult : MoolreResult
{
    internal BankAccountListResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        int count,
        IReadOnlyList<BankAccountSummary> accounts)
        : base(code, message, messages, httpStatusCode)
    {
        Count = count;
        Accounts = accounts;
    }

    /// <summary>Count. Moolre field <c>data.txcount</c> (an integer here).</summary>
    public int Count { get; }

    /// <summary>Accounts. Moolre field <c>data.accounts</c>.</summary>
    public IReadOnlyList<BankAccountSummary> Accounts { get; }
}
