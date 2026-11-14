namespace Zhuwa.Moolre.Webhooks;

/// <summary>A payment notification that Moolre sends to your callback URL.</summary>
public class PaymentWebhook
{
    private readonly bool _wireStatusIsSuccess;

    internal PaymentWebhook(
        MoolreResponseCode code,
        string? message,
        PaymentWebhookData data,
        bool wireStatusIsSuccess)
    {
        Code = code;
        Message = message;
        Data = data;
        _wireStatusIsSuccess = wireStatusIsSuccess;
    }

    /// <summary>The payment outcome code: <c>P01</c> successful or <c>P02</c> failed. Not an HTTP status. Moolre field <c>code</c>.</summary>
    public MoolreResponseCode Code { get; }

    /// <summary>Moolre's outcome message, such as "Transaction Successful". Moolre field <c>message</c>.</summary>
    public string? Message { get; }

    /// <summary>The transaction details. Moolre field <c>data</c>.</summary>
    public PaymentWebhookData Data { get; }

    /// <summary>
    /// Whether this is a successful payment: status 1, code <c>P01</c> and <c>txstatus</c> 1, all three.
    /// A failed callback (<c>P02</c>) must never be used to fulfil an order.
    /// </summary>
    public bool IsSuccessfulPayment =>
        _wireStatusIsSuccess
        && Code == MoolreResponseCode.PaymentSucceeded
        && Data.Status == TransactionStatus.Successful;
}
