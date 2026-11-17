using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the create bank account operation.</summary>
public class CreateBankAccountRequest
{
    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (a JSON NUMBER on this endpoint only).</summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    /// <summary>First name. Moolre field <c>firstname</c>.</summary>
    [JsonPropertyName("firstname")]
    public required string FirstName { get; init; }

    /// <summary>Last name. Moolre field <c>lastname</c>.</summary>
    [JsonPropertyName("lastname")]
    public required string LastName { get; init; }

    /// <summary>Phone number. Moolre field <c>phone</c>.</summary>
    [JsonPropertyName("phone")]
    public required string PhoneNumber { get; init; }

    /// <summary>Email. Moolre field <c>email</c>.</summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
