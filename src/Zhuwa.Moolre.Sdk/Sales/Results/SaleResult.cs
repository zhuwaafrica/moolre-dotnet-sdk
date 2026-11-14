namespace Zhuwa.Moolre.Sales;

/// <summary>Result of the sale operation.</summary>
public class SaleResult : MoolreResult
{
    internal SaleResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        TransactionStatus status,
        string receiver,
        string transactionId,
        string externalReference,
        decimal amount)
        : base(code, message, messages, httpStatusCode)
    {
        Status = status;
        Receiver = receiver;
        TransactionId = transactionId;
        ExternalReference = externalReference;
        Amount = amount;
    }

    /// <summary>Status. Moolre field <c>data.txstatus</c> (0 = in progress).</summary>
    public TransactionStatus Status { get; }

    /// <summary>Receiver. Moolre field <c>data.receiver</c>.</summary>
    public string Receiver { get; }

    /// <summary>Transaction ID. Moolre field <c>data.transactionid</c> ("Pending" while in progress).</summary>
    public string TransactionId { get; }

    /// <summary>External reference. Moolre field <c>data.externalref</c>.</summary>
    public string ExternalReference { get; }

    /// <summary>Amount. Moolre field <c>data.amount</c>.</summary>
    public decimal Amount { get; }

    /// <summary>Whether Moolre reported the sale as in progress (code ASL11). In progress is not delivered.</summary>
    public bool IsInProgress => Code == MoolreResponseCode.SaleInProgress;
}
