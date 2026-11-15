using System.Runtime.CompilerServices;
using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>
/// Sends the payment calls. Each method checks the request, writes the body, sends it through the transport and decides which
/// answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolrePayments(MoolreTransport transport) : MoolrePayments
{
    public override async Task<MobileMoneyResult> RequestMobileMoneyAsync(RequestMobileMoneyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PayerPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        var channel = ToWire(request.Channel, nameof(request.Channel));
        var currency = request.Currency.ToWire(nameof(request.Currency));

        // skipotp only exists in the sandbox; sending it to the live API could change how the payment is verified.
        if (request.SkipOtp is not null && !transport.IsSandbox)
        {
            throw new ArgumentException("SkipOtp only works in the sandbox. Leave it unset for live payments.", nameof(request));
        }

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(RequestMobileMoneyRequest));
        var externalReference = request.ExternalReference.Trim();

        // The reference also goes to the transport: if the outcome becomes unknown, the exception carries it so the caller checks
        // the status instead of charging the customer twice.
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.RequestMobileMoney,
            writer =>
            {
                writer.WriteString("channel", channel);
                writer.WriteString("currency", currency);
                writer.WriteString("payer", request.PayerPhoneNumber.Trim());
                writer.WriteString("amount", request.Amount.ToWire());
                writer.WriteString("externalref", externalReference);
                WriteOptional(writer, "otpcode", request.OtpCode);
                WriteOptional(writer, "reference", request.Reference);
                WriteOptional(writer, "sessionid", request.SessionId);
                writer.WriteString("accountnumber", accountNumber);
                if (request.SkipOtp is bool skipOtp)
                {
                    writer.WriteBoolean("skipotp", skipOtp);
                }
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);

        // "OTP required" arrives with status 1, so it passes this check and comes back as a result. A status 0 answer (for example
        // an invalid currency) means Moolre refused it and nothing was charged; the duplicate reference was thrown by the transport.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToMobileMoneyResult();
    }

    public override async Task<TransactionResult> GetStatusAsync(GetPaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        var idType = request.IdKind.ToWire(nameof(request.IdKind));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetPaymentStatusRequest));

        // The same request as transfer status; the endpoint entry picks the public key.
        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.GetStatus,
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

    public override async Task<PaymentLinkResult> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        var currency = request.Currency.ToWire(nameof(request.Currency));
        var expiryMinutes = ToWholeMinutes(request.ExpiresAfter);
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreatePaymentLinkRequest));
        var externalReference = request.ExternalReference.Trim();

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.CreatePaymentLink,
            writer =>
            {
                writer.WriteString("amount", request.Amount.ToWire());
                writer.WriteString("email", request.Email.Trim());
                writer.WriteString("externalref", externalReference);
                if (request.CallbackUrl is not null)
                {
                    writer.WriteString("callback", request.CallbackUrl.AbsoluteUri);
                }

                if (request.RedirectUrl is not null)
                {
                    writer.WriteString("redirect", request.RedirectUrl.AbsoluteUri);
                }

                // Moolre wants "1" or "0" as text here, not a JSON boolean.
                writer.WriteString("reusable", request.IsReusable ? "1" : "0");
                if (expiryMinutes is int minutes)
                {
                    writer.WriteNumber("expiration_time", minutes);
                }

                writer.WriteString("currency", currency);
                writer.WriteString("accountnumber", accountNumber);
                WriteMetadata(writer, request.Metadata);
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);

        // The duplicate reference (INP02) was thrown by the transport; any other status 0 is a refusal.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentLinkResult();
    }

    public override async Task<PaymentIdResult> CreatePaymentIdAsync(CreatePaymentIdRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);

        // Moolre documents this endpoint with international numbers (+233...), unlike collections, which use the local 0... form.
        var phone = request.PhoneNumber.Trim();
        if (!phone.StartsWith('+'))
        {
            throw new ArgumentException("Use the international format for a Payment ID phone number, for example +233267606822.", nameof(request));
        }

        if (request.Amount is decimal presetAmount)
        {
            RequestGuard.ThrowIfInvalidAmount(presetAmount, nameof(request.Amount));
        }

        var currency = request.Currency.ToWire(nameof(request.Currency));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreatePaymentIdRequest));
        var externalReference = request.ExternalReference.Trim();

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.CreatePaymentId,
            writer =>
            {
                writer.WriteString("phone", phone);
                writer.WriteString("name", request.Name.Trim());
                writer.WriteString("currency", currency);
                if (request.Amount is decimal amount)
                {
                    writer.WriteString("amount", amount.ToWire());
                }

                writer.WriteString("externalref", externalReference);
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentIdResult();
    }

    public override async Task<BankAccountResult> CreateBankAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
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

        var currency = request.Currency.ToWire(nameof(request.Currency));
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(CreateBankAccountRequest));
        var externalReference = request.ExternalReference.Trim();

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.CreateBankAccount,
            writer =>
            {
                writer.WriteString("currency", currency);

                // This endpoint documents the amount as a JSON number, unlike every other amount, which is text.
                if (request.Amount is decimal amount)
                {
                    writer.WriteNumber("amount", amount);
                }

                writer.WriteString("firstname", request.FirstName.Trim());
                writer.WriteString("lastname", request.LastName.Trim());
                writer.WriteString("phone", request.PhoneNumber.Trim());
                writer.WriteString("email", request.Email.Trim());
                writer.WriteString("externalref", externalReference);
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference,
            cancellationToken).ConfigureAwait(false);

        // AD32 ("account could not be created") arrives with status 0 and becomes MoolreApiException here.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToBankAccountResult(externalReference);
    }

    public override async Task<BankAccountListResult> ListBankAccountsAsync(ListBankAccountsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ListBankAccountsRequest));

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.ListBankAccounts,
            writer => writer.WriteString("accountnumber", accountNumber),
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToBankAccountListResult();
    }

    public override async Task<PaymentAccountLookupResult> GetPaymentAccountAsync(GetPaymentAccountRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetPaymentAccountRequest));

        var envelope = await transport.SendAsync(
            MoolreEndpoints.Payments.GetPaymentAccount,
            writer =>
            {
                writer.WriteString("externalref", request.ExternalReference.Trim());
                writer.WriteString("accountnumber", accountNumber);
            },
            query: null,
            externalReference: null,
            cancellationToken).ConfigureAwait(false);

        // Not found (AC03) arrives with status 1, so it comes back as a result with Found = false.
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToPaymentAccountLookupResult();
    }

    // Moolre's collection channel codes. MTN is 13 here (not 1 as for transfers); the named values used by sales are not accepted.
    private static string ToWire(PaymentChannel channel, string paramName) => channel switch
    {
        PaymentChannel.Mtn => "13",
        PaymentChannel.Telecel => "6",
        PaymentChannel.At => "7",
        _ => throw new ArgumentOutOfRangeException(paramName, channel, "Choose a payment channel: Mtn, Telecel or At."),
    };

    // Moolre takes the link expiry in whole minutes, at least one. A part minute is refused, not rounded, so the link never
    // lives longer or shorter than the caller asked.
    private static int? ToWholeMinutes(TimeSpan? expiresAfter, [CallerArgumentExpression(nameof(expiresAfter))] string? paramName = null)
    {
        if (expiresAfter is not TimeSpan value)
        {
            return null;
        }

        if (value < TimeSpan.FromMinutes(1) || value.Ticks % TimeSpan.TicksPerMinute != 0 || value.TotalMinutes > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "The expiry must be a whole number of minutes, at least one.");
        }

        return (int)value.TotalMinutes;
    }

    // Optional text fields are left out when blank rather than sent empty.
    private static void WriteOptional(Utf8JsonWriter writer, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteString(name, value.Trim());
        }
    }

    // Metadata is sent as a JSON object of text values, and only when there is any.
    private static void WriteMetadata(Utf8JsonWriter writer, IDictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
        {
            return;
        }

        writer.WriteStartObject("metadata");
        foreach (var pair in metadata)
        {
            writer.WriteString(pair.Key, pair.Value);
        }

        writer.WriteEndObject();
    }
}
