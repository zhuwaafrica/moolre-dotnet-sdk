using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the get SMS status operation.</summary>
public class GetSmsStatusRequest
{
    /// <summary>References. Moolre field <c>ref</c> (array of message references).</summary>
    [JsonPropertyName("ref")]
    public Collection<string> References { get; } = [];
}
