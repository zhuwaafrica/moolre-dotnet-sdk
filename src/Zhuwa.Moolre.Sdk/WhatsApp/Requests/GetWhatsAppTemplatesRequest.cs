namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Parameters for the get WhatsApp templates operation.</summary>
public class GetWhatsAppTemplatesRequest
{
    /// <summary>Cursor. Moolre field <c>next</c> (opaque; omit for the first page).</summary>
    public string? Cursor { get; init; }
}
