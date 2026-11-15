using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>
/// Sends the transfer calls. Each method checks the request, writes the body, sends it through the transport and decides which
/// answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolreTransfers(MoolreTransport transport) : MoolreTransfers
{
    public override async Task<NameValidationResult> ValidateNameAsync(ValidateNameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Receiver);
        var channel = ToWire(request.Channel, nameof(request.Channel));
        if (request.Channel == TransferChannel.Bank)
        {
            RequestGuard.ThrowIfBlank(request.BankCode, "A bank account needs the bank code (from ReferenceData.GetBanksAsync).");
        }

        var currency = request.Currency.ToWire(nameof(request.Currency));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ValidateNameRequest));

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Transfers.ValidateName,
            writer =>
            {
                writer.WriteString("receiver", request.Receiver.Trim());
                writer.WriteString("channel", channel);
                WriteBankCode(writer, request.Channel, request.BankCode);
                writer.WriteString("currency", currency);
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);

        // "Not found" is an answer to the question, not a failure of the call, so it comes back as a result.
        if (envelope.Code != MoolreResponseCode.NameNotFound)
        {
            MoolreTransport.EnsureSuccess(envelope);
        }

        return envelope.ToNameValidationResult();
    }

    public override async Task<TransferResult> SendAsync(SendTransferRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Receiver);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        var channel = ToWire(request.Channel, nameof(request.Channel));
        if (request.Channel == TransferChannel.Bank)
        {
            RequestGuard.ThrowIfBlank(request.BankCode, "A bank transfer needs the bank code (from ReferenceData.GetBanksAsync).");
        }

        var currency = request.Currency.ToWire(nameof(request.Currency));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendTransferRequest));
        var externalReference = request.ExternalReference.Trim();

        // The external reference goes to the transport too: if the outcome becomes unknown, the exception carries it so the caller
        // can look the transfer up instead of paying twice.
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Transfers.Send,
            writer =>
            {
                writer.WriteString("channel", channel);
                writer.WriteString("currency", currency);
                writer.WriteString("amount", request.Amount.ToWire());
                writer.WriteString("receiver", request.Receiver.Trim());
                WriteBankCode(writer, request.Channel, request.BankCode);
                writer.WriteString("externalref", externalReference);
                WriteNarration(writer, request.Narration);
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);

        // "In progress" is not a refusal: the money may still move, so it is returned as a pending result. Any other status 0
        // (payout failed, insufficient balance) means Moolre refused it and no money moved.
        if (envelope.Code != MoolreResponseCode.PayoutInProgress)
        {
            MoolreTransport.EnsureSuccess(envelope);
        }

        return envelope.ToTransferResult(request);
    }

    public override async Task<TransactionResult> GetStatusAsync(GetTransferStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        var idType = request.IdKind.ToWire(nameof(request.IdKind));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetTransferStatusRequest));

        // The same request as payment status; the endpoint entry picks the transfer credential (either key).
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Transfers.GetStatus,
            writer =>
            {
                writer.WriteNumber("idtype", idType);
                writer.WriteString("id", request.Id.Trim());
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToTransactionResult();
    }

    public override async Task<InternalTransferResult> SendInternalAsync(SendInternalTransferRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReceiverAccountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        var currency = request.Currency.ToWire(nameof(request.Currency));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendInternalTransferRequest));
        var externalReference = request.ExternalReference.Trim();

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Transfers.SendInternal,
            writer =>
            {
                writer.WriteString("currency", currency);
                writer.WriteString("amount", request.Amount.ToWire());
                writer.WriteString("receiver", request.ReceiverAccountNumber.Trim());
                writer.WriteString("externalref", externalReference);
                WriteNarration(writer, request.Narration);
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);

        // Insufficient balance (TR09) and other status 0 answers are refusals; the duplicate reference (TR013) was already thrown by
        // the transport.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToInternalTransferResult();
    }

    // Moolre's transfer channel codes. They are numbers sent as text; the named values used by sales are not accepted here.
    private static string ToWire(TransferChannel channel, string paramName) => channel switch
    {
        TransferChannel.Mtn => "1",
        TransferChannel.Bank => "2",
        TransferChannel.Telecel => "6",
        TransferChannel.At => "7",
        _ => throw new ArgumentOutOfRangeException(paramName, channel, "Choose a transfer channel: Mtn, Telecel, At or Bank."),
    };

    // The bank code is only meaningful for bank transfers; it is left out otherwise so Moolre never sees a stray value.
    private static void WriteBankCode(Utf8JsonWriter writer, TransferChannel channel, string? bankCode)
    {
        if (channel == TransferChannel.Bank)
        {
            writer.WriteString("sublistid", bankCode!.Trim());
        }
    }

    // Moolre calls the narration "reference". It is optional, so a blank value is left out rather than sent empty.
    private static void WriteNarration(Utf8JsonWriter writer, string? narration)
    {
        if (!string.IsNullOrWhiteSpace(narration))
        {
            writer.WriteString("reference", narration.Trim());
        }
    }
}
