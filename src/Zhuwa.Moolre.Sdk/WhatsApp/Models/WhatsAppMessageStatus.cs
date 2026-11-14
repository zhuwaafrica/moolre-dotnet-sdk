namespace Zhuwa.Moolre.WhatsApp;

/// <summary>WhatsApp message status.</summary>
public class WhatsAppMessageStatus
{
    internal WhatsAppMessageStatus(
        string reference,
        WhatsAppDeliveryStatus status)
    {
        Reference = reference;
        Status = status;
    }

    /// <summary>Reference. Moolre field <c>ref</c>.</summary>
    public string Reference { get; }

    /// <summary>Status. Moolre field <c>status</c>.</summary>
    public WhatsAppDeliveryStatus Status { get; }
}
