using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Reference;

/// <summary>The query for Moolre's bank list (<c>?country=gha&amp;data=banks</c>). Internal: callers pass only the country code.</summary>
internal sealed class GetBanksRequest
{
    /// <summary>Moolre's country code, for example <c>gha</c>.</summary>
    [JsonPropertyName("country")]
    public required string CountryCode { get; init; }

    /// <summary>Which reference list to return; this call always asks for banks.</summary>
    [JsonPropertyName("data")]
    public string Data { get; } = "banks";
}
