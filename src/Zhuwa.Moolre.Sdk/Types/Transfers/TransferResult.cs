namespace Zhuwa.Moolre;

/// <summary>Result of the transfer operation.</summary>
public class TransferResult : MoolreResult
{
    internal TransferResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        TransactionStatus status,
        string receiver,
        string transactionId,
        string externalReference,
        string thirdPartyReference,
        string receiverName,
        decimal amount,
        decimal amountWithFee,
        decimal networkFee,
        decimal fee)
        : base(code, message, messages, httpStatusCode)
    {
        Status = status;
        Receiver = receiver;
        TransactionId = transactionId;
        ExternalReference = externalReference;
        ThirdPartyReference = thirdPartyReference;
        ReceiverName = receiverName;
        Amount = amount;
        AmountWithFee = amountWithFee;
        NetworkFee = networkFee;
        Fee = fee;
    }

    /// <summary>
    /// The transfer's state. Moolre field <c>data.txstatus</c>. <see cref="TransactionStatus.Pending"/> when Moolre reports
    /// "pay out in progress" (<c>OTGH02</c>) or sends no status: confirm with <see cref="Transfers.MoolreTransfers.GetStatusAsync"/>.
    /// </summary>
    public TransactionStatus Status { get; }

    /// <summary>Receiver. Moolre field <c>data.receiver</c>.</summary>
    public string Receiver { get; }

    /// <summary>Transaction ID. Moolre field <c>data.transactionid</c>.</summary>
    public string TransactionId { get; }

    /// <summary>External reference. Moolre field <c>data.externalref</c>.</summary>
    public string ExternalReference { get; }

    /// <summary>Third party reference. Moolre field <c>data.thirdpartyref</c>.</summary>
    public string ThirdPartyReference { get; }

    /// <summary>Receiver name. Moolre field <c>data.receivername</c>.</summary>
    public string ReceiverName { get; }

    /// <summary>Amount. Moolre field <c>data.amount</c>.</summary>
    public decimal Amount { get; }

    /// <summary>Amount with fee. Moolre field <c>data.amountfee</c>.</summary>
    public decimal AmountWithFee { get; }

    /// <summary>Network fee. Moolre field <c>data.networkfee</c>.</summary>
    public decimal NetworkFee { get; }

    /// <summary>Fee. Moolre field <c>data.fee</c>.</summary>
    public decimal Fee { get; }
}
