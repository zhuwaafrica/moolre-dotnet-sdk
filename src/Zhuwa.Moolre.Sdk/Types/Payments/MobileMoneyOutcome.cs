namespace Zhuwa.Moolre;

/// <summary>Mobile money outcome.</summary>
public enum MobileMoneyOutcome
{
    /// <summary>Moolre answered with a code the SDK does not know. Check <see cref="MoolreResult.Code"/> and the payment status.</summary>
    None = 0,

    /// <summary>Prompt sent. TR099: prompt triggered; RequestId holds the UUID</summary>
    PromptSent = 1,

    /// <summary>OTP required. TP14 (arrives with status 1): resend with OtpCode</summary>
    OtpRequired = 2,

    /// <summary>Phone verified. TP17 (body not documented)</summary>
    PhoneVerified = 3,
}
