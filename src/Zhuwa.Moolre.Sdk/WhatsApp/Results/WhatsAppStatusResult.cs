namespace Zhuwa.Moolre.WhatsApp;

/// <summary>Result of the WhatsApp status operation.</summary>
public class WhatsAppStatusResult : MoolreResult
{
    internal WhatsAppStatusResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<WhatsAppMessageStatus> statuses)
        : base(code, message, messages, httpStatusCode)
    {
        Statuses = statuses;
    }

    /// <summary>Statuses. Moolre field <c>data</c>.</summary>
    public IReadOnlyList<WhatsAppMessageStatus> Statuses { get; }
}
