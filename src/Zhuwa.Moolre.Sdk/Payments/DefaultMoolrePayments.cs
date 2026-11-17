using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>
/// Sends the payment calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it
/// through the transport and decides which answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolrePayments(MoolreTransport transport) : MoolrePayments
{
    public override async Task<MobileMoneyResult> RequestMobileMoneyAsync(RequestMobileMoneyRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request, transport);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(RequestMobileMoneyRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.RequestMobileMoneyRequest).SetAccountNumber(accountNumber);

        // The reference also goes to the transport: if the outcome becomes unknown, the exception carries it so the caller checks
        // the status instead of charging the customer twice.
        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.RequestMobileMoney, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        // "OTP required" arrives with status 1, so it passes this check and comes back as a result. A status 0 answer (for example an
        // invalid currency) means Moolre refused it and nothing was charged; the duplicate reference was thrown by the transport.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToMobileMoneyResult();
    }

    public override async Task<TransactionResult> GetStatusAsync(GetPaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetPaymentStatusRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetPaymentStatusRequest).SetAccountNumber(accountNumber);

        // The same request as transfer status; the endpoint entry picks the public key.
        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.GetStatus, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToTransactionResult();
    }

    public override async Task<PaymentLinkResult> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreatePaymentLinkRequest));

        // "reusable" is written as "1"/"0" text and the expiry as whole minutes, by the converters named on the request type.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.CreatePaymentLinkRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.CreatePaymentLink, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        // The duplicate reference (INP02) was thrown by the transport; any other status 0 is a refusal.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentLinkResult();
    }

    public override async Task<PaymentIdResult> CreatePaymentIdAsync(CreatePaymentIdRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreatePaymentIdRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.CreatePaymentIdRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.CreatePaymentId, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentIdResult();
    }

    public override async Task<BankAccountResult> CreateBankAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreateBankAccountRequest));

        // This endpoint documents the amount as a JSON number (every other amount is text); the request type writes it that way.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.CreateBankAccountRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.CreateBankAccount, body, request.ExternalReference.Trim(), cancellationToken)
            .ConfigureAwait(false);

        // AD32 ("account could not be created") arrives with status 0 and becomes MoolreApiException here.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToBankAccountResult(request.ExternalReference.Trim());
    }

    public override async Task<BankAccountListResult> ListBankAccountsAsync(ListBankAccountsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ListBankAccountsRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.ListBankAccountsRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.ListBankAccounts, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToBankAccountListResult();
    }

    public override async Task<PaymentAccountLookupResult> GetPaymentAccountAsync(GetPaymentAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetPaymentAccountRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetPaymentAccountRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Payments.GetPaymentAccount, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // Not found (AC03) arrives with status 1, so it comes back as a result with Found = false.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentAccountLookupResult();
    }

    // Checks a mobile money request: payer, reference, amount, channel and currency, and SkipOtp only in the sandbox. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(RequestMobileMoneyRequest request, MoolreTransport transport)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PayerPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        _ = request.Channel.ToWire(nameof(request.Channel));
        _ = request.Currency.ToWire(nameof(request.Currency));

        // skipotp only exists in the sandbox; sending it to the live API could change how the payment is verified.
        if (request.SkipOtp is not null && !transport.IsSandbox)
        {
            throw new ArgumentException("SkipOtp only works in the sandbox. Leave it unset for live payments.", nameof(request));
        }
    }

    // Checks a payment status request: an ID and its kind. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(GetPaymentStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        _ = request.IdKind.ToWire(nameof(request.IdKind));
    }

    // Checks a payment link request: email, reference, amount, expiry in whole minutes, http or https callback and redirect URLs, and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(CreatePaymentLinkRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        RequestGuard.ThrowIfNotWholeMinutes(request.ExpiresAfter);
        RequestGuard.ThrowIfNotWebUrl(request.CallbackUrl);
        RequestGuard.ThrowIfNotWebUrl(request.RedirectUrl);
        _ = request.Currency.ToWire(nameof(request.Currency));
    }

    // Checks a Payment ID request: phone in international format, name, reference, optional preset amount and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(CreatePaymentIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);

        // Moolre documents this endpoint with international numbers (+233...), unlike collections, which use the local 0... form.
        if (!request.PhoneNumber.Trim().StartsWith('+'))
        {
            throw new ArgumentException("Use the international format for a Payment ID phone number, for example +233267606822.", nameof(request));
        }

        if (request.Amount is decimal presetAmount)
        {
            RequestGuard.ThrowIfInvalidAmount(presetAmount, nameof(request.Amount));
        }

        _ = request.Currency.ToWire(nameof(request.Currency));
    }

    // Checks a bank account request: the holder's details, reference, optional preset amount and currency. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(CreateBankAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FirstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        if (request.Amount is decimal presetAmount)
        {
            RequestGuard.ThrowIfInvalidAmount(presetAmount, nameof(request.Amount));
        }

        _ = request.Currency.ToWire(nameof(request.Currency));
    }

    // Checks a Payment ID or bank account lookup: the reference used to create it. Runs before anything is sent, so a bad request never reaches Moolre.
    private static void Validate(GetPaymentAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
    }
}
