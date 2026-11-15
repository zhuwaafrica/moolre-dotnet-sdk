namespace Zhuwa.Moolre;

/// <summary>Result of the sale status operation.</summary>
public class SaleStatusResult : MoolreResult
{
    internal SaleStatusResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        SaleStatus status,
        string? product,
        string? network,
        string? bundleId,
        string? receiver,
        decimal? amount,
        decimal? discount,
        decimal? charged,
        string? accountNumber,
        string? transactionId,
        string? refundId,
        string? saleReference,
        string? externalReference,
        string? thirdPartyReference,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt)
        : base(code, message, messages, httpStatusCode)
    {
        Status = status;
        Product = product;
        Network = network;
        BundleId = bundleId;
        Receiver = receiver;
        Amount = amount;
        Discount = discount;
        Charged = charged;
        AccountNumber = accountNumber;
        TransactionId = transactionId;
        RefundId = refundId;
        SaleReference = saleReference;
        ExternalReference = externalReference;
        ThirdPartyReference = thirdPartyReference;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>Status. Moolre field <c>data.txstatus</c>.</summary>
    public SaleStatus Status { get; }

    /// <summary>Product. Moolre field <c>data.product</c> ("data").</summary>
    public string? Product { get; }

    /// <summary>Network. Moolre field <c>data.network</c> ("mtn").</summary>
    public string? Network { get; }

    /// <summary>Bundle ID. Moolre field <c>data.bundle</c> (data sales)    OPEN: airtime variant not documented.</summary>
    public string? BundleId { get; }

    /// <summary>Receiver. Moolre field <c>data.receiver</c>.</summary>
    public string? Receiver { get; }

    /// <summary>Amount. Moolre field <c>data.amount</c>.</summary>
    public decimal? Amount { get; }

    /// <summary>Discount. Moolre field <c>data.discount</c>.</summary>
    public decimal? Discount { get; }

    /// <summary>Charged. Moolre field <c>data.charged</c>.</summary>
    public decimal? Charged { get; }

    /// <summary>Account number. Moolre field <c>data.accountnumber</c> (masked in the example).</summary>
    public string? AccountNumber { get; }

    /// <summary>Transaction ID. Moolre field <c>data.transactionid</c>.</summary>
    public string? TransactionId { get; }

    /// <summary>Refund ID. Moolre field <c>data.refundid</c> (null unless refunded).</summary>
    public string? RefundId { get; }

    /// <summary>Sale reference. Moolre field <c>data.reference</c>.</summary>
    public string? SaleReference { get; }

    /// <summary>External reference. Moolre field <c>data.externalref</c>.</summary>
    public string? ExternalReference { get; }

    /// <summary>Third party reference. Moolre field <c>data.thirdpartyref</c>.</summary>
    public string? ThirdPartyReference { get; }

    /// <summary>Created at. Moolre field <c>data.ts</c>.</summary>
    public DateTimeOffset? CreatedAt { get; }

    /// <summary>Updated at. Moolre field <c>data.updated</c>.</summary>
    public DateTimeOffset? UpdatedAt { get; }
}
