namespace Zhuwa.Moolre.WhatsApp;

/// <summary>WhatsApp rejected message.</summary>
public class WhatsAppRejectedMessage
{
    internal WhatsAppRejectedMessage(
        string recipient,
        string reference)
    {
        Recipient = recipient;
        Reference = reference;
    }

    /// <summary>Recipient. Moolre field <c>data[].recipient</c>.</summary>
    public string Recipient { get; }

    /// <summary>Reference. Moolre field <c>data[].ref</c>.</summary>
    public string Reference { get; }
}
