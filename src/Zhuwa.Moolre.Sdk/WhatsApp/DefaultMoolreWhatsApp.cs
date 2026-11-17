using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>
/// Sends the WhatsApp calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it through
/// the transport and decides which answers are results and which are failures; the mapper turns the answer into the result. WhatsApp calls
/// have no <c>type</c> field; the endpoint table leaves it out.
/// </summary>
internal sealed class DefaultMoolreWhatsApp(MoolreTransport transport) : MoolreWhatsApp
{
    public override async Task<WhatsAppTemplatesPage> GetTemplatePageAsync(GetWhatsAppTemplatesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // A GET call: the cursor (when there is one) becomes the query string, escaped.
        var query = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetWhatsAppTemplatesRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.WhatsApp.GetTemplatePage, query, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToTemplatesPage();
    }

    public override async Task<WhatsAppSendResult> SendAsync(SendWhatsAppMessagesRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        // A bulk send has no single reference; an unknown outcome is still reported, and the caller checks the messages' own references.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendWhatsAppMessagesRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.WhatsApp.Send, body, externalReference: null, cancellationToken).ConfigureAwait(false);

        // Insufficient balance (WAS401) arrives with status 0 and becomes MoolreApiException; nothing was sent.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToSendResult();
    }

    public override async Task<WhatsAppStatusResult> GetStatusAsync(GetWhatsAppMessageStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.References.Count == 0 || request.References.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Add at least one message reference, none blank.", nameof(request));
        }

        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetWhatsAppMessageStatusRequest);
        var envelope = await transport.SendAsync(MoolreEndpoints.WhatsApp.GetStatus, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToStatusResult();
    }

    // Checks a send: a template name and language, and at least one message with a recipient. Placeholder counts cannot be checked here,
    // because the template's own placeholders are not known to this call; Moolre checks them.
    private static void Validate(SendWhatsAppMessagesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TemplateName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Language);
        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("Add at least one message.", nameof(request));
        }

        foreach (var message in request.Messages)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(message.Recipient, nameof(request));
        }
    }
}
