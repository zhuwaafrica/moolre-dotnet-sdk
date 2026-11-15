using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for looking up the status of a payment, by your external reference or by Moolre's ID.</summary>
public class GetPaymentStatusRequest
{
    /// <summary>Which identifier <see cref="Id"/> is. Moolre field <c>idtype</c>.</summary>
    [JsonPropertyName("idtype")]
    public required TransactionIdKind IdKind { get; init; }

    /// <summary>Your external reference or Moolre's transaction ID. Moolre field <c>id</c>.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The wallet; the default from <see cref="MoolreOptions.AccountNumber"/> when omitted. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
