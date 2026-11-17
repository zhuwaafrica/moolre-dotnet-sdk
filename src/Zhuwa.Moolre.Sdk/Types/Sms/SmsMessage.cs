using System.Text.Json.Serialization;
namespace Zhuwa.Moolre;

/// <summary>SMS message.</summary>
public class SmsMessage
{
    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    [JsonPropertyName("recipient")]
    public required string Recipient { get; init; }

    /// <summary>Text. Moolre field <c>message</c>.</summary>
    [JsonPropertyName("message")]
    public required string Text { get; init; }

    /// <summary>Reference. Moolre field <c>ref</c> (needed to look up delivery status later).</summary>
    [JsonPropertyName("ref")]
    public string? Reference { get; init; }
}
