using System.Collections.ObjectModel;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Parameters for the send WhatsApp messages operation.</summary>
public class SendWhatsAppMessagesRequest
{
    /// <summary>Template name. Moolre field <c>template_name</c> (an approved template).</summary>
    public required string TemplateName { get; init; }

    /// <summary>Language. Moolre field <c>language</c> (English only).</summary>
    public required string Language { get; init; }

    /// <summary>Messages. Moolre field <c>messages</c>.</summary>
    public Collection<WhatsAppMessage> Messages { get; } = [];
}
