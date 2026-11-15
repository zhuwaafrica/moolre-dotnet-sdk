using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for looking up the status of a transfer, by your external reference or by Moolre's ID.</summary>
public class GetTransferStatusRequest
{
    /// <summary>ID kind. Moolre field <c>idtype</c>.</summary>
    [JsonPropertyName("idtype")]
    public required TransactionIdKind IdKind { get; init; }

    /// <summary>ID. Moolre field <c>id</c>.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
