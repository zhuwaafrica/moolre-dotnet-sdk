using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>Where and how often Moolre settles a wallet's collections.</summary>
public class SettlementSettings
{
    /// <summary>Currency. Moolre field <c>settlement.currency</c> ("GHS").</summary>
    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(EmptyWhenNullStringConverter))]
    public string? Currency { get; set; }

    /// <summary>Frequency. Moolre field <c>settlement.frequency</c> ("1"); empty when not configured.</summary>
    [JsonPropertyName("frequency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(EmptyWhenNullStringConverter))]
    public string? Frequency { get; set; }

    /// <summary>Channel. Moolre field <c>settlement.channel</c> ("1").</summary>
    [JsonPropertyName("channel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(EmptyWhenNullStringConverter))]
    public string? Channel { get; set; }

    /// <summary>Recipient. Moolre field <c>settlement.recipient</c> (phone or account).</summary>
    [JsonPropertyName("recipient")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(EmptyWhenNullStringConverter))]
    public string? Recipient { get; set; }

    /// <summary>Bank code. Moolre field <c>settlement.sublist</c>.</summary>
    [JsonPropertyName("sublist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(EmptyWhenNullStringConverter))]
    public string? BankCode { get; set; }
}
