namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for looking up the status of a payment or a transfer (Moolre uses the same request for both).</summary>
public class GetTransferStatusRequest
{
    /// <summary>ID kind. Moolre field <c>idtype</c>.</summary>
    public required TransactionIdKind IdKind { get; init; }

    /// <summary>ID. Moolre field <c>id</c>.</summary>
    public required string Id { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
