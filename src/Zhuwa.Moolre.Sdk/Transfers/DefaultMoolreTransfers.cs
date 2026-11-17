using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Transfers;

/// <summary>
/// Sends the transfer calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it
/// through the transport and decides which answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolreTransfers(MoolreTransport transport) : MoolreTransfers
{
    public override async Task<NameValidationResult> ValidateNameAsync(ValidateNameRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ValidateNameRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.ValidateNameRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Transfers.ValidateName, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // "Not found" is an answer to the question, not a failure of the call, so it comes back as a result.
        if (envelope.Code != MoolreResponseCode.NameNotFound)
        {
            MoolreTransport.EnsureSuccess(envelope);
        }

        return envelope.ToNameValidationResult();
    }

    public override async Task<TransferResult> SendAsync(SendTransferRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendTransferRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendTransferRequest).SetAccountNumber(accountNumber);

        // The external reference goes to the transport too: if the outcome becomes unknown, the exception carries it so the caller
        // can look the transfer up instead of paying twice.
        var envelope = await transport.SendAsync(MoolreEndpoints.Transfers.Send, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);

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
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetTransferStatusRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetTransferStatusRequest).SetAccountNumber(accountNumber);

        // The same request as payment status; the endpoint entry picks the transfer credential (either key).
        var envelope = await transport.SendAsync(MoolreEndpoints.Transfers.GetStatus, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToTransactionResult();
    }

    public override async Task<InternalTransferResult> SendInternalAsync(SendInternalTransferRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendInternalTransferRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendInternalTransferRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Transfers.SendInternal, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        // Insufficient balance (TR09) and other status 0 answers are refusals; the duplicate reference (TR013) was already thrown by
        // the transport.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToInternalTransferResult();
    }

    // Checks a name lookup: receiver, channel (with the bank code for a bank account) and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(ValidateNameRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Receiver);
        _ = request.Channel.ToWire(nameof(request.Channel));
        if (request.Channel == TransferChannel.Bank)
        {
            RequestGuard.ThrowIfBlank(request.BankCode, "A bank account needs the bank code (from ReferenceData.GetBanksAsync).");
        }

        _ = request.Currency.ToWire(nameof(request.Currency));
    }

    // Checks a transfer: receiver, reference, amount, channel (with the bank code for a bank transfer) and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(SendTransferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Receiver);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        _ = request.Channel.ToWire(nameof(request.Channel));
        if (request.Channel == TransferChannel.Bank)
        {
            RequestGuard.ThrowIfBlank(request.BankCode, "A bank transfer needs the bank code (from ReferenceData.GetBanksAsync).");
        }

        _ = request.Currency.ToWire(nameof(request.Currency));
    }

    // Checks a transfer status request: an ID and its kind. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(GetTransferStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        _ = request.IdKind.ToWire(nameof(request.IdKind));
    }

    // Checks an internal transfer: receiving wallet, reference, amount and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(SendInternalTransferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReceiverAccountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        _ = request.Currency.ToWire(nameof(request.Currency));
    }
}
