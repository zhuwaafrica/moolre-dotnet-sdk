using System.Collections.ObjectModel;

namespace Zhuwa.Moolre;

/// <summary>WhatsApp message.</summary>
public class WhatsAppMessage
{
    /// <summary>Recipient. Moolre field <c>recipient</c>.</summary>
    public required string Recipient { get; init; }

    /// <summary>Reference. Moolre field <c>ref</c>.</summary>
    public string? Reference { get; init; }

    /// <summary>Placeholders. Moolre field <c>placeholders</c> (values for {{1}}, {{2}}, …).</summary>
    public Collection<string> Placeholders { get; } = [];
}
