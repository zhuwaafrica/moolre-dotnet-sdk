namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for the send internal transfer operation.</summary>
public class SendInternalTransferRequest
{
    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    public required decimal Amount { get; init; }

    /// <summary>Receiver account number. Moolre field <c>receiver</c> (a Moolre account number, not a phone).</summary>
    public required string ReceiverAccountNumber { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    public string? Narration { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
