namespace Zhuwa.Moolre;

/// <summary>Sender ID decision.</summary>
public class SenderIdDecision
{
    /// <summary>Sender ID. Moolre field <c>senderid</c>.</summary>
    public required string SenderId { get; init; }

    /// <summary>Decision. Moolre field <c>approve</c>.</summary>
    public required SenderIdDecisionKind Decision { get; init; }
}
