namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for the validate name operation.</summary>
public class ValidateNameRequest
{
    /// <summary>Receiver. Moolre field <c>receiver</c> (phone or bank account number).</summary>
    public required string Receiver { get; init; }

    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    public required TransferChannel Channel { get; init; }

    /// <summary>Bank code. Moolre field <c>sublistid</c> (required when Channel is Bank).</summary>
    public string? BankCode { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
