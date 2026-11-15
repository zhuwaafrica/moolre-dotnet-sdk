namespace Zhuwa.Moolre;

// WhatsApp send codes.
// One file per API area keeps this list readable as Moolre adds codes; it is still one type for callers.
public readonly partial struct MoolreResponseCode
{
    /// <summary>WhatsApp success (<c>WAS200</c>).</summary>
    public static MoolreResponseCode WhatsAppSuccess { get; } = new("WAS200");

    /// <summary>WhatsApp insufficient balance (<c>WAS401</c>).</summary>
    public static MoolreResponseCode WhatsAppInsufficientBalance { get; } = new("WAS401");
}
