namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for the send transfer operation.</summary>
public class SendTransferRequest
{
    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    public required TransferChannel Channel { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    public required decimal Amount { get; init; }

    /// <summary>Receiver. Moolre field <c>receiver</c> (mobile number starting with 0, or bank account number).</summary>
    public required string Receiver { get; init; }

    /// <summary>Bank code. Moolre field <c>sublistid</c> (required for Bank).</summary>
    public string? BankCode { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    public string? Narration { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
