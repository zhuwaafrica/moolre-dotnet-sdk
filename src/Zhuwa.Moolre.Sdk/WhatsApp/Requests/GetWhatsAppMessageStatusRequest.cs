using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Parameters for the get WhatsApp message status operation.</summary>
public class GetWhatsAppMessageStatusRequest
{
    /// <summary>References. Moolre field <c>ref</c> (array).</summary>
    [JsonPropertyName("ref")]
    public Collection<string> References { get; } = [];
}
