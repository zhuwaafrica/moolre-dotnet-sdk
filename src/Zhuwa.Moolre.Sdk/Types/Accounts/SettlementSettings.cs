namespace Zhuwa.Moolre;

/// <summary>Where and how often Moolre settles a wallet's collections.</summary>
public class SettlementSettings
{
    /// <summary>Currency. Moolre field <c>settlement.currency</c> ("GHS").</summary>
    public string? Currency { get; set; }

    /// <summary>Frequency. Moolre field <c>settlement.frequency</c> ("1"); empty when not configured.</summary>
    public string? Frequency { get; set; }

    /// <summary>Channel. Moolre field <c>settlement.channel</c> ("1").</summary>
    public string? Channel { get; set; }

    /// <summary>Recipient. Moolre field <c>settlement.recipient</c> (phone or account).</summary>
    public string? Recipient { get; set; }

    /// <summary>Bank code. Moolre field <c>settlement.sublist</c>.</summary>
    public string? BankCode { get; set; }
}
