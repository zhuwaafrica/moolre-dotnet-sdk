using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>
/// Turns Moolre's payment responses into the public result types. Field names live on the response models (for example
/// <see cref="PaymentLinkWire"/>).
/// </summary>
internal static class PaymentsMapper
{
    /// <summary>
    /// Maps a mobile money response. The outcome comes from the code: <c>TR099</c> prompt sent (<c>data</c> is the request ID),
    /// <c>TP14</c> OTP required, <c>TP17</c> phone verified. Any other code reads as <see cref="MobileMoneyOutcome.None"/>, so the
    /// caller never mistakes an unknown answer for a sent prompt.
    /// </summary>
    internal static MobileMoneyResult ToMobileMoneyResult(this ResponseEnvelope envelope)
    {
        MobileMoneyOutcome outcome;
        if (envelope.Code == MoolreResponseCode.TransferInitiated)
        {
            outcome = MobileMoneyOutcome.PromptSent;
        }
        else if (envelope.Code == MoolreResponseCode.OtpRequired)
        {
            outcome = MobileMoneyOutcome.OtpRequired;
        }
        else if (envelope.Code == MoolreResponseCode.PhoneVerified)
        {
            outcome = MobileMoneyOutcome.PhoneVerified;
        }
        else
        {
            outcome = MobileMoneyOutcome.None;
        }

        // Only a sent prompt carries an ID; with TP14 data is the word "all", which must not look like an ID.
        var requestId = outcome == MobileMoneyOutcome.PromptSent ? MoolreJson.ReadDataText(envelope.Data) : null;

        return new MobileMoneyResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, outcome, requestId);
    }

    /// <summary>Maps a payment link response.</summary>
    /// <exception cref="MoolreResponseException">Moolre reported success but sent no absolute link; a result without a link is useless.</exception>
    internal static PaymentLinkResult ToPaymentLinkResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.PaymentLinkWire, envelope.HttpStatusCode);
        var link = wire.AuthorizationUrl
            ?? throw new MoolreResponseException("Moolre reported a payment link but sent no usable link.", envelope.HttpStatusCode);

        return new PaymentLinkResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, link, wire.Reference ?? string.Empty);
    }

    /// <summary>Maps a Payment ID response.</summary>
    internal static PaymentIdResult ToPaymentIdResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.PaymentIdWire, envelope.HttpStatusCode);

        return new PaymentIdResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            wire.PaymentId ?? string.Empty,
            wire.Name ?? string.Empty,
            wire.QrCode);
    }

    /// <summary>Maps a created bank account response.</summary>
    /// <param name="envelope">The parsed response.</param>
    /// <param name="requestedReference">The request's reference, used when Moolre does not echo it back.</param>
    internal static BankAccountResult ToBankAccountResult(this ResponseEnvelope envelope, string requestedReference)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.BankAccountWire, envelope.HttpStatusCode);

        return new BankAccountResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            wire.BankAccountNumber ?? string.Empty,
            wire.AccountName ?? string.Empty,
            wire.BankName ?? string.Empty,
            wire.ExternalReference ?? requestedReference);
    }

    /// <summary>Maps a bank account list.</summary>
    internal static BankAccountListResult ToBankAccountListResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.BankAccountListWire, envelope.HttpStatusCode);
        var accounts = wire.Accounts
            .Select(a => new BankAccountSummary(
                a.BankAccountNumber ?? string.Empty,
                a.AccountName ?? string.Empty,
                a.BankName ?? string.Empty,
                a.ExternalReference ?? string.Empty,
                a.CreatedAt))
            .ToList();

        // txcount is Moolre's total; the number of accounts received fills in when it is missing.
        return new BankAccountListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, wire.Count ?? accounts.Count, accounts);
    }

    /// <summary>
    /// Maps a Payment ID or bank account lookup. The account is null when not found, and the transactions arrive as an array, a single
    /// object or <c>{}</c>; the response model turns all three into a list.
    /// </summary>
    internal static PaymentAccountLookupResult ToPaymentAccountLookupResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.PaymentAccountLookupWire, envelope.HttpStatusCode);
        var account = wire.Account is { } a
            ? new PaymentAccountSummary(a.BankAccountNumber ?? string.Empty, a.TransactionCount, a.TransactionValue)
            : null;

        return new PaymentAccountLookupResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            account,
            wire.Transactions.Select(t => t.ToTransaction()).ToList());
    }
}
