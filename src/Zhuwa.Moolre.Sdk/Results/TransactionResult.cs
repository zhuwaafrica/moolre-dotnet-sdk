namespace Zhuwa.Moolre;

/// <summary>A single transaction returned by a status lookup.</summary>
public class TransactionResult : MoolreResult
{
    internal TransactionResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        MoolreTransaction transaction)
        : base(code, message, messages, httpStatusCode)
    {
        Transaction = transaction;
    }

    /// <summary>Transaction. Moolre field <c>data</c>.</summary>
    public MoolreTransaction Transaction { get; }
}
