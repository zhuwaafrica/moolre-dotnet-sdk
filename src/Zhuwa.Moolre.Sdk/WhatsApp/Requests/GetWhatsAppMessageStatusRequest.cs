using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Parameters for the get WhatsApp message status operation.</summary>
public class GetWhatsAppMessageStatusRequest
{
    /// <summary>References. Moolre field <c>ref</c> (array).</summary>
    public Collection<string> References { get; } = [];
}
