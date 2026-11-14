namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the request mobile money operation.</summary>
public class RequestMobileMoneyRequest
{
    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    public required PaymentChannel Channel { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Payer phone number. Moolre field <c>payer</c> (local 0-prefixed format).</summary>
    public required string PayerPhoneNumber { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    public required decimal Amount { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>OTP code. Moolre field <c>otpcode</c> (answer to an OTP challenge).</summary>
    public string? OtpCode { get; init; }

    /// <summary>Reference. Moolre field <c>reference</c>.</summary>
    public string? Reference { get; init; }

    /// <summary>Session ID. Moolre field <c>sessionid</c> (USSD session).</summary>
    public string? SessionId { get; init; }

    /// <summary>Skip OTP. Moolre field <c>skipotp</c> (sandbox only).</summary>
    public bool? SkipOtp { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
