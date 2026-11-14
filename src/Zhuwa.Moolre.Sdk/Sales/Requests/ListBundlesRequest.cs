namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the list bundles operation.</summary>
public class ListBundlesRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Network. Moolre field <c>network</c> (omit for all networks).</summary>
    public SalesNetwork? Network { get; init; }
}
