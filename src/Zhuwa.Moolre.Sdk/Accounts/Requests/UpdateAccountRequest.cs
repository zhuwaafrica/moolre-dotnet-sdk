using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the update account operation.</summary>
public class UpdateAccountRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }

    // Moolre requires the currency on every update and accepts only GHS, so it is always sent and not exposed.
    [JsonInclude]
    [JsonPropertyName("currency")]
    internal string WireCurrency { get; } = "GHS";

    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    [JsonPropertyName("accountname")]
    public string? AccountName { get; init; }

    /// <summary>API enabled. Moolre field <c>api</c> (send only to change API access).</summary>
    [JsonPropertyName("api")]
    public bool? ApiEnabled { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    [JsonPropertyName("callback")]
    public Uri? CallbackUrl { get; init; }

    /// <summary>Settlement. Moolre field <c>settlement</c>.</summary>
    [JsonPropertyName("settlement")]
    public SettlementSettings? Settlement { get; init; }
}
