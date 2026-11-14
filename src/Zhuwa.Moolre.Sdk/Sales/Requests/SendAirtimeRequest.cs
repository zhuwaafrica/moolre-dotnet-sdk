namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the send airtime operation.</summary>
public class SendAirtimeRequest
{
    /// <summary>Network. Moolre field <c>channel</c>.</summary>
    public required SalesNetwork Network { get; init; }

    /// <summary>Receiver phone number. Moolre field <c>receiver</c>.</summary>
    public required string ReceiverPhoneNumber { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string, GHS, minimum 1.00 — enforced by Moolre).</summary>
    public required decimal Amount { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (unique per wallet).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    public string? Narration { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c> (must be linked to the Sell on Our Platform service).</summary>
    public string? AccountNumber { get; init; }
}
