using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the create sender ids operation.</summary>
public class CreateSenderIdsRequest
{
    /// <summary>Sender ids. Moolre field <c>senderids</c>.</summary>
    [JsonPropertyName("senderids")]
    public Collection<SenderIdRegistration> SenderIds { get; } = [];
}
