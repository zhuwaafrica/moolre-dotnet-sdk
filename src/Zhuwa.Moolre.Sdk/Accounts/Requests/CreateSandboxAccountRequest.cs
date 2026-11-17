using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the create sandbox account operation.</summary>
public class CreateSandboxAccountRequest
{
    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    [JsonPropertyName("accountname")]
    public required string AccountName { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c> (GHS only).</summary>
    [JsonPropertyName("currency")]
    public Currency Currency { get; init; } = Currency.Ghs;

    /// <summary>API enabled. Moolre field <c>api</c>.</summary>
    [JsonPropertyName("api")]
    public bool? ApiEnabled { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    [JsonPropertyName("callback")]
    public required Uri CallbackUrl { get; init; }

    /// <summary>Settlement. Moolre field <c>settlement</c>.</summary>
    [JsonPropertyName("settlement")]
    public required SettlementSettings Settlement { get; init; }
}
