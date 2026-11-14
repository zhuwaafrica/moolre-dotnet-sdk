namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the get payment account operation.</summary>
public class GetPaymentAccountRequest
{
    /// <summary>External reference. Moolre field <c>externalref</c> (used when creating the Payment ID or bank account).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
