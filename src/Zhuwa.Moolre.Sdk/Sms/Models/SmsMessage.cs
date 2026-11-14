namespace Zhuwa.Moolre.Sms;

/// <summary>SMS message.</summary>
public class SmsMessage
{
    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    public required string Recipient { get; init; }

    /// <summary>Text. Moolre field <c>message</c>.</summary>
    public required string Text { get; init; }

    /// <summary>Reference. Moolre field <c>ref</c> (needed to look up delivery status later).</summary>
    public string? Reference { get; init; }
}
