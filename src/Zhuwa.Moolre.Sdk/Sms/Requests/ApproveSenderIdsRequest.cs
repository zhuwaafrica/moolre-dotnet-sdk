using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the approve sender ids operation.</summary>
public class ApproveSenderIdsRequest
{
    /// <summary>Decisions. Moolre field <c>senderids</c>.</summary>
    [JsonPropertyName("senderids")]
    public Collection<SenderIdDecision> Decisions { get; } = [];
}
