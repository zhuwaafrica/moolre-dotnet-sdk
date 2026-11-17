using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the request mobile money operation.</summary>
public class RequestMobileMoneyRequest
{
    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    [JsonPropertyName("channel")]
    public required PaymentChannel Channel { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    /// <summary>Payer phone number. Moolre field <c>payer</c> (local 0-prefixed format).</summary>
    [JsonPropertyName("payer")]
    public required string PayerPhoneNumber { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalAsTextConverter))]
    public required decimal Amount { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>OTP code. Moolre field <c>otpcode</c> (answer to an OTP challenge).</summary>
    [JsonPropertyName("otpcode")]
    public string? OtpCode { get; init; }

    /// <summary>Reference. Moolre field <c>reference</c>.</summary>
    [JsonPropertyName("reference")]
    public string? Reference { get; init; }

    /// <summary>Session ID. Moolre field <c>sessionid</c> (USSD session).</summary>
    [JsonPropertyName("sessionid")]
    public string? SessionId { get; init; }

    /// <summary>Skip OTP. Moolre field <c>skipotp</c> (sandbox only).</summary>
    [JsonPropertyName("skipotp")]
    public bool? SkipOtp { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
