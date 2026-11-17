using System.Text.Json.Serialization;
namespace Zhuwa.Moolre;

/// <summary>Sender ID registration.</summary>
public class SenderIdRegistration
{
    /// <summary>Sender ID. Moolre field <c>senderid</c> (max 11 characters).</summary>
    [JsonPropertyName("senderid")]
    public required string SenderId { get; init; }

    /// <summary>Approve. Moolre field <c>approve</c> (needs permission).</summary>
    [JsonPropertyName("approve")]
    public bool? Approve { get; init; }
}
