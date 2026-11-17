using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the list bundles operation.</summary>
public class ListBundlesRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }

    /// <summary>Network. Moolre field <c>network</c> (omit for all networks).</summary>
    [JsonPropertyName("network")]
    public SalesNetwork? Network { get; init; }
}
