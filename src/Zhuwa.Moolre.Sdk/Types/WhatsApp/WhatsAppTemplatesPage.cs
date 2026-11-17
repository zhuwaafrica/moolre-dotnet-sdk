namespace Zhuwa.Moolre;

/// <summary>One page of WhatsApp templates.</summary>
public class WhatsAppTemplatesPage : MoolreResult
{
    internal WhatsAppTemplatesPage(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<WhatsAppTemplate> templates,
        string? nextCursor)
        : base(code, message, messages, httpStatusCode)
    {
        Templates = templates;
        NextCursor = nextCursor;
    }

    /// <summary>Templates. Moolre field <c>data</c>.</summary>
    public IReadOnlyList<WhatsAppTemplate> Templates { get; }

    /// <summary>Next cursor. Moolre field <c>next</c> (null on the last page).</summary>
    public string? NextCursor { get; }
}
