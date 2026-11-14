using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the approve sender ids operation.</summary>
public class ApproveSenderIdsRequest
{
    /// <summary>Decisions. Moolre field <c>senderids</c>.</summary>
    public Collection<SenderIdDecision> Decisions { get; } = [];
}
