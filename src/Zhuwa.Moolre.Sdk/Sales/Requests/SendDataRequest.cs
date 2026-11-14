namespace Zhuwa.Moolre.Sales;

/// <summary>Parameters for the send data operation.</summary>
public class SendDataRequest
{
    /// <summary>Network. Moolre field <c>channel</c>.</summary>
    public required SalesNetwork Network { get; init; }

    /// <summary>Receiver phone number. Moolre field <c>receiver</c>.</summary>
    public required string ReceiverPhoneNumber { get; init; }

    /// <summary>Bundle ID. Moolre field <c>bundle</c> (an id returned by ListBundlesAsync, passed unchanged).</summary>
    public required string BundleId { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (unique per wallet).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    public string? Narration { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
