using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for the validate name operation.</summary>
public class ValidateNameRequest
{
    /// <summary>Receiver. Moolre field <c>receiver</c> (phone or bank account number).</summary>
    [JsonPropertyName("receiver")]
    public required string Receiver { get; init; }

    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    [JsonPropertyName("channel")]
    public required TransferChannel Channel { get; init; }

    /// <summary>Bank code. Moolre field <c>sublistid</c> (required when Channel is Bank).</summary>
    [JsonIgnore]
    public string? BankCode { get; init; }

    // Moolre's "sublistid", sent only for bank accounts, so Moolre never sees a stray bank code on a mobile money call.
    [JsonInclude]
    [JsonPropertyName("sublistid")]
    internal string? WireBankCode => Channel == TransferChannel.Bank ? BankCode : null;

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
