namespace Zhuwa.Moolre.Payments;

/// <summary>Result of the payment ID operation.</summary>
public class PaymentIdResult : MoolreResult
{
    internal PaymentIdResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string paymentId,
        string name,
        string? qrCode)
        : base(code, message, messages, httpStatusCode)
    {
        PaymentId = paymentId;
        Name = name;
        QrCode = qrCode;
    }

    /// <summary>Payment ID. Moolre field <c>data.paymentid</c>.</summary>
    public string PaymentId { get; }

    /// <summary>Name. Moolre field <c>data.name</c>.</summary>
    public string Name { get; }

    /// <summary>QR code. Moolre field <c>data.qrcode</c> (null in the example).</summary>
    public string? QrCode { get; }
}
