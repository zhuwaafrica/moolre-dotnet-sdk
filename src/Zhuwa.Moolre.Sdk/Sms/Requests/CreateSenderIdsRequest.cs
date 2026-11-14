using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the create sender ids operation.</summary>
public class CreateSenderIdsRequest
{
    /// <summary>Sender ids. Moolre field <c>senderids</c>.</summary>
    public Collection<SenderIdRegistration> SenderIds { get; } = [];
}
