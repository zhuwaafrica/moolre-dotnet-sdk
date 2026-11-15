using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Parameters for the send WhatsApp messages operation.</summary>
public class SendWhatsAppMessagesRequest
{
    /// <summary>Template name. Moolre field <c>template_name</c> (an approved template).</summary>
    [JsonPropertyName("template_name")]
    public required string TemplateName { get; init; }

    /// <summary>Language. Moolre field <c>language</c> (English only).</summary>
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    /// <summary>Messages. Moolre field <c>messages</c>.</summary>
    [JsonPropertyName("messages")]
    public Collection<WhatsAppMessage> Messages { get; } = [];
}
