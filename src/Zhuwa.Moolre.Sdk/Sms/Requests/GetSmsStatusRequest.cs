using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.Sms;

/// <summary>Parameters for the get SMS status operation.</summary>
public class GetSmsStatusRequest
{
    /// <summary>References. Moolre field <c>ref</c> (array of message references).</summary>
    public Collection<string> References { get; } = [];
}
