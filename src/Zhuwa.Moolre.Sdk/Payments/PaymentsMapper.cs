using System.Text.Json;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Turns Moolre's payment responses into the public result types (static, hand-written, no reflection).</summary>
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
        var requestId = outcome == MobileMoneyOutcome.PromptSent && envelope.Data.ValueKind == JsonValueKind.String
            ? envelope.Data.GetString()
            : null;

        return new MobileMoneyResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, outcome, requestId);
    }

    /// <summary>Maps a payment link response, whose <c>data</c> holds <c>authorization_url</c> and <c>reference</c>.</summary>
    /// <exception cref="MoolreResponseException">Moolre reported success but sent no absolute link; a result without a link is useless.</exception>
    internal static PaymentLinkResult ToPaymentLinkResult(this ResponseEnvelope envelope)
    {
        var data = envelope.Data;
        var link = data.ReadUri("authorization_url")
            ?? throw new MoolreResponseException("Moolre reported a payment link but sent no usable authorization_url.", envelope.HttpStatusCode);

        return new PaymentLinkResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            link,
            data.ReadString("reference") ?? string.Empty);
    }

    /// <summary>Maps a Payment ID response, whose <c>data</c> holds <c>paymentid</c>, <c>name</c> and <c>qrcode</c>.</summary>
    internal static PaymentIdResult ToPaymentIdResult(this ResponseEnvelope envelope)
    {
        var data = envelope.Data;

        return new PaymentIdResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            data.ReadString("paymentid") ?? string.Empty,
            data.ReadString("name") ?? string.Empty,
            data.ReadString("qrcode"));
    }

    /// <summary>Maps a created bank account response, whose <c>data</c> holds the new account's number, name and bank.</summary>
    internal static BankAccountResult ToBankAccountResult(this ResponseEnvelope envelope, string requestedReference)
    {
        var data = envelope.Data;

        return new BankAccountResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            data.ReadString("accountno") ?? string.Empty,
            data.ReadString("accountname") ?? string.Empty,
            data.ReadString("bankname") ?? string.Empty,
            // Moolre echoes the reference; the request's value fills in if it does not.
            data.ReadString("externalref") ?? requestedReference);
    }

    /// <summary>Maps a bank account list, whose <c>data</c> holds <c>txcount</c> and an <c>accounts</c> array.</summary>
    internal static BankAccountListResult ToBankAccountListResult(this ResponseEnvelope envelope)
    {
        var accounts = new List<BankAccountSummary>();
        if (envelope.Data.TryGetField("accounts", out var items) && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
            {
                accounts.Add(new BankAccountSummary(
                    item.ReadString("accountno") ?? string.Empty,
                    item.ReadString("accountname") ?? string.Empty,
                    item.ReadString("bankname") ?? string.Empty,
                    item.ReadString("extid") ?? string.Empty,
                    item.ReadTimestamp("ts")));
            }
        }

        // txcount is Moolre's total; fall back to the number of accounts received when it is missing.
        var count = envelope.Data.ReadInt32("txcount", fallback: accounts.Count);

        return new BankAccountListResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, count, accounts);
    }

    /// <summary>
    /// Maps a Payment ID or bank account lookup. <c>data.account</c> is null when not found, and <c>data.transactions</c> is an array
    /// (list pages), a single object (status page) or <c>{}</c> (not found); all three become a list.
    /// </summary>
    internal static PaymentAccountLookupResult ToPaymentAccountLookupResult(this ResponseEnvelope envelope)
    {
        var data = envelope.Data;

        PaymentAccountSummary? account = null;
        if (data.TryGetField("account", out var accountElement) && accountElement.ValueKind == JsonValueKind.Object)
        {
            account = new PaymentAccountSummary(
                accountElement.ReadString("accountno") ?? string.Empty,
                accountElement.ReadInt32("txcount"),
                accountElement.ReadDecimal("txvalue"));
        }

        var transactions = new List<MoolreTransaction>();
        if (data.TryGetField("transactions", out var transactionsElement))
        {
            if (transactionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in transactionsElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        transactions.Add(item.ToTransaction());
                    }
                }
            }
            else if (transactionsElement.ValueKind == JsonValueKind.Object && transactionsElement.EnumerateObject().Any())
            {
                // A single transaction object; the empty object {} means "none".
                transactions.Add(transactionsElement.ToTransaction());
            }
        }

        return new PaymentAccountLookupResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, account, transactions);
    }
}
