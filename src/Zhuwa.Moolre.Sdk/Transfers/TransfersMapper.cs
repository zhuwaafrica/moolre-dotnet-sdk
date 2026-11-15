using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>Turns Moolre's transfer responses into the public result types. Field names live on <see cref="TransferWire"/>.</summary>
internal static class TransfersMapper
{
    /// <summary>
    /// Maps a validate name response. On <c>AVD01</c> the account holder's name is <c>data</c> (plain text). On <c>AVD02</c> (not
    /// found) <c>data</c> repeats the error text, so the name is left empty instead of showing "Phone No. not found" as a name.
    /// </summary>
    internal static NameValidationResult ToNameValidationResult(this ResponseEnvelope envelope)
    {
        var name = envelope.IsSuccess ? MoolreJson.ReadDataText(envelope.Data) ?? string.Empty : string.Empty;

        return new NameValidationResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, name.Trim());
    }

    /// <summary>Maps a send transfer response.</summary>
    /// <param name="envelope">The parsed response.</param>
    /// <param name="request">The request that was sent; its values fill the result when Moolre does not echo them back.</param>
    internal static TransferResult ToTransferResult(this ResponseEnvelope envelope, SendTransferRequest request)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.TransferWire, envelope.HttpStatusCode);

        // "Pay out in progress" means Moolre has not finished; report pending whatever txstatus says, so the caller checks the status
        // later instead of treating the transfer as done.
        var status = envelope.Code == MoolreResponseCode.PayoutInProgress ? TransactionStatus.Pending : wire.Status;

        return new TransferResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            status,
            wire.Receiver ?? request.Receiver.Trim(),
            wire.TransactionId ?? string.Empty,
            wire.ExternalReference ?? request.ExternalReference.Trim(),
            wire.ThirdPartyReference ?? string.Empty,
            wire.ReceiverName ?? string.Empty,
            wire.Amount,
            wire.AmountWithFee,
            wire.NetworkFee,
            wire.Fee);
    }

    /// <summary>Maps an internal transfer response, whose <c>data</c> is the transfer's ID (a UUID as text).</summary>
    internal static InternalTransferResult ToInternalTransferResult(this ResponseEnvelope envelope) =>
        new(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, MoolreJson.ReadDataText(envelope.Data) ?? string.Empty);
}
