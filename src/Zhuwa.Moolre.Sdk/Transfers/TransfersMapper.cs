using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>Turns Moolre's transfer responses into the public result types (static, hand-written, no reflection).</summary>
internal static class TransfersMapper
{
    /// <summary>
    /// Maps a validate name response. On <c>AVD01</c> the account holder's name is <c>data</c> (a plain string). On <c>AVD02</c>
    /// (not found) <c>data</c> repeats the error text, so the name is left empty instead of showing "Phone No. not found" as a name.
    /// </summary>
    internal static NameValidationResult ToNameValidationResult(this ResponseEnvelope envelope)
    {
        var name = envelope.IsSuccess && envelope.Data.ValueKind == JsonValueKind.String
            ? envelope.Data.GetString() ?? string.Empty
            : string.Empty;

        return new NameValidationResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, name.Trim());
    }

    /// <summary>Maps a send transfer response.</summary>
    /// <param name="envelope">The parsed response.</param>
    /// <param name="request">The request that was sent; its reference fills the result when Moolre does not echo it back.</param>
    internal static TransferResult ToTransferResult(this ResponseEnvelope envelope, SendTransferRequest request)
    {
        var data = envelope.Data;

        // "Pay out in progress" means Moolre has not finished; report pending whatever txstatus says, so the caller checks the status
        // later instead of treating the transfer as done.
        var status = envelope.Code == MoolreResponseCode.PayoutInProgress
            ? TransactionStatus.Pending
            : data.ReadTransactionStatus("txstatus");

        return new TransferResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            status,
            data.ReadString("receiver") ?? request.Receiver,
            data.ReadString("transactionid") ?? string.Empty,
            data.ReadString("externalref") ?? request.ExternalReference,
            data.ReadString("thirdpartyref") ?? string.Empty,
            data.ReadString("receivername") ?? string.Empty,
            data.ReadDecimal("amount"),
            data.ReadDecimal("amountfee"),
            data.ReadDecimal("networkfee"),
            data.ReadDecimal("fee"));
    }

    /// <summary>Maps an internal transfer response, whose <c>data</c> is the transfer's ID (a UUID string).</summary>
    internal static InternalTransferResult ToInternalTransferResult(this ResponseEnvelope envelope)
    {
        var transferId = envelope.Data.ValueKind == JsonValueKind.String ? envelope.Data.GetString() ?? string.Empty : string.Empty;

        return new InternalTransferResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, transferId);
    }
}
