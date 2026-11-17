using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Turns Moolre's WhatsApp responses into the public result types. Field names live on the response models (for example <see cref="WhatsAppTemplateWire"/>).</summary>
internal static class WhatsAppMapper
{
    /// <summary>Maps a templates page: <c>data</c> is the templates, and the top-level <c>next</c> is the cursor for the next page.</summary>
    internal static WhatsAppTemplatesPage ToTemplatesPage(this ResponseEnvelope envelope)
    {
        var templates = MoolreJson.EnumerateData(envelope.Data, MoolreJsonContext.Default.WhatsAppTemplateWire)
            .Select(t => new WhatsAppTemplate(
                t.Id ?? string.Empty,
                t.Name ?? string.Empty,
                t.Language ?? string.Empty,
                string.IsNullOrWhiteSpace(t.Status) ? default : new WhatsAppTemplateStatus(t.Status),
                t.Body ?? string.Empty,
                // A placeholder that is not text reads as null and is left out.
                t.Placeholders.Where(p => !string.IsNullOrEmpty(p)).Select(p => p!).ToList()))
            .ToList();

        return new WhatsAppTemplatesPage(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, templates, envelope.Next);
    }

    /// <summary>
    /// Maps a send response. A full success has <c>error: []</c> and <c>data: []</c>. When some references were not unique, Moolre still
    /// answers with status 1 but puts a warning in <c>error</c> and the skipped messages in <c>data</c>; both are reported.
    /// </summary>
    internal static WhatsAppSendResult ToSendResult(this ResponseEnvelope envelope)
    {
        var warning = envelope.Error.ValueKind == JsonValueKind.String && envelope.Error.GetString() is { Length: > 0 } text ? text : null;
        var notSent = MoolreJson.EnumerateData(envelope.Data, MoolreJsonContext.Default.WhatsAppRejectedWire)
            .Select(m => new WhatsAppRejectedMessage(m.Recipient ?? string.Empty, m.Reference ?? string.Empty))
            .ToList();

        return new WhatsAppSendResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, warning, notSent);
    }

    /// <summary>Maps a status response, whose <c>data</c> is an array of <c>{ ref, status }</c>; statuses are kept exactly as text.</summary>
    internal static WhatsAppStatusResult ToStatusResult(this ResponseEnvelope envelope)
    {
        var statuses = MoolreJson.EnumerateData(envelope.Data, MoolreJsonContext.Default.WhatsAppStatusWire)
            .Select(s => new WhatsAppMessageStatus(
                s.Reference ?? string.Empty,
                string.IsNullOrWhiteSpace(s.Status) ? default : new WhatsAppDeliveryStatus(s.Status)))
            .ToList();

        return new WhatsAppStatusResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, statuses);
    }
}
