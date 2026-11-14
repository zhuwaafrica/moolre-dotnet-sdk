namespace Zhuwa.Moolre;

/// <summary>One transaction as Moolre reports it: account lists, payment and transfer status, and Payment ID or bank account lookups.</summary>
public class MoolreTransaction
{
    internal MoolreTransaction(
        TransactionStatus status,
        int transactionType,
        string accountNumber,
        string payer,
        string payee,
        decimal amount,
        decimal value,
        decimal? fee,
        decimal? networkFee,
        string transactionId,
        string externalReference,
        string thirdPartyReference,
        DateTimeOffset timestamp)
    {
        Status = status;
        TransactionType = transactionType;
        AccountNumber = accountNumber;
        Payer = payer;
        Payee = payee;
        Amount = amount;
        Value = value;
        Fee = fee;
        NetworkFee = networkFee;
        TransactionId = transactionId;
        ExternalReference = externalReference;
        ThirdPartyReference = thirdPartyReference;
        Timestamp = timestamp;
    }

    /// <summary>Status. Moolre field <c>txstatus</c>.</summary>
    public TransactionStatus Status { get; }

    /// <summary>Transaction type. Moolre field <c>txtype</c> OPEN: meaning of 1 vs 2 is not documented.</summary>
    public int TransactionType { get; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string AccountNumber { get; }

    /// <summary>Payer. Moolre field <c>payer</c> (may be empty).</summary>
    public string Payer { get; }

    /// <summary>Payee. Moolre field <c>payee</c>.</summary>
    public string Payee { get; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    public decimal Amount { get; }

    /// <summary>Value. Moolre field <c>value</c> (string; amount after fees).</summary>
    public decimal Value { get; }

    /// <summary>Fee. Moolre field <c>fee</c> (Payment ID / bank account lookups only).</summary>
    public decimal? Fee { get; }

    /// <summary>Network fee. Moolre field <c>networkfee</c> (same lookups only).</summary>
    public decimal? NetworkFee { get; }

    /// <summary>Transaction ID. Moolre field <c>transactionid</c>.</summary>
    public string TransactionId { get; }

    /// <summary>External reference. Moolre field <c>externalref</c> ("0" when none).</summary>
    public string ExternalReference { get; }

    /// <summary>Third party reference. Moolre field <c>thirdpartyref</c>.</summary>
    public string ThirdPartyReference { get; }

    /// <summary>Timestamp. Moolre field <c>ts</c> "YYYY-MM-DD HH:mm:ss" (GMT; documented for webhooks only).</summary>
    public DateTimeOffset Timestamp { get; }
}
