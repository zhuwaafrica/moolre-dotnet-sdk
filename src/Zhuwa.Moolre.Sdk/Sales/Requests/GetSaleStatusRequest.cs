namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the get sale status operation.</summary>
public class GetSaleStatusRequest
{
    /// <summary>ID kind. Moolre field <c>idtype</c>.</summary>
    public required SaleIdKind IdKind { get; init; }

    /// <summary>ID. Moolre field <c>id</c>.</summary>
    public required string Id { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
