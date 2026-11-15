using System.Text.Json.Serialization;
namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the get sender ID status operation.</summary>
public class GetSenderIdStatusRequest
{
    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    [JsonPropertyName("senderid")]
    public required string SenderId { get; init; }
}
