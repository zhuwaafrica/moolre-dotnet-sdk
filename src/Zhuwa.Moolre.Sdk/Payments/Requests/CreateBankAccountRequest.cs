namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the create bank account operation.</summary>
public class CreateBankAccountRequest
{
    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (a JSON NUMBER on this endpoint only).</summary>
    public decimal? Amount { get; init; }

    /// <summary>First name. Moolre field <c>firstname</c>.</summary>
    public required string FirstName { get; init; }

    /// <summary>Last name. Moolre field <c>lastname</c>.</summary>
    public required string LastName { get; init; }

    /// <summary>Phone number. Moolre field <c>phone</c>.</summary>
    public required string PhoneNumber { get; init; }

    /// <summary>Email. Moolre field <c>email</c>.</summary>
    public required string Email { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
