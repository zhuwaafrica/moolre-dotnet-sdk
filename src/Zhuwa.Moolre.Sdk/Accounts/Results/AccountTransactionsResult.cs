namespace Zhuwa.Moolre.Accounts;

/// <summary>Result of the account transactions operation.</summary>
public class AccountTransactionsResult : MoolreResult
{
    internal AccountTransactionsResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        int transactionCount,
        IReadOnlyList<MoolreTransaction> transactions)
        : base(code, message, messages, httpStatusCode)
    {
        TransactionCount = transactionCount;
        Transactions = transactions;
    }

    /// <summary>Transaction count. Moolre field <c>data.txcount</c> (string).</summary>
    public int TransactionCount { get; }

    /// <summary>Transactions. Moolre field <c>data.transactions</c> (empty for ST09).</summary>
    public IReadOnlyList<MoolreTransaction> Transactions { get; }
}
