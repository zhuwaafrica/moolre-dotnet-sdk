using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the get payment account operation.</summary>
public class GetPaymentAccountRequest
{
    /// <summary>External reference. Moolre field <c>externalref</c> (used when creating the Payment ID or bank account).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
