using System.Text.Json.Serialization;
namespace Zhuwa.Moolre;

/// <summary>Sender ID decision.</summary>
public class SenderIdDecision
{
    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    [JsonPropertyName("senderid")]
    public required string SenderId { get; init; }

    /// <summary>Decision. Moolre field <c>approve</c>.</summary>
    [JsonPropertyName("approve")]
    public required SenderIdDecisionKind Decision { get; init; }
}
