using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>Parameters for the send transfer operation.</summary>
public class SendTransferRequest
{
    /// <summary>Channel. Moolre field <c>channel</c>.</summary>
    [JsonPropertyName("channel")]
    public required TransferChannel Channel { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalAsTextConverter))]
    public required decimal Amount { get; init; }

    /// <summary>Receiver. Moolre field <c>receiver</c> (mobile number starting with 0, or bank account number).</summary>
    [JsonPropertyName("receiver")]
    public required string Receiver { get; init; }

    /// <summary>Bank code. Moolre field <c>sublistid</c> (required for Bank).</summary>
    [JsonIgnore]
    public string? BankCode { get; init; }

    // Moolre's "sublistid", sent only for bank transfers, so Moolre never sees a stray bank code on a mobile money call.
    [JsonInclude]
    [JsonPropertyName("sublistid")]
    internal string? WireBankCode => Channel == TransferChannel.Bank ? BankCode : null;

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>Narration. Moolre field <c>reference</c>.</summary>
    [JsonPropertyName("reference")]
    public string? Narration { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }
}
