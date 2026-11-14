namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the create payment ID operation.</summary>
public class CreatePaymentIdRequest
{
    /// <summary>Phone number. Moolre field <c>phone</c> (+233 format, e.g. +233267606822).</summary>
    public required string PhoneNumber { get; init; }

    /// <summary>Name. Moolre field <c>name</c>.</summary>
    public required string Name { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (optional preset, greater than zero when sent).</summary>
    public decimal? Amount { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
