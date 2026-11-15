using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Accounts;

/// <summary>
/// Turns Moolre's account responses into the public result types. The field names live on the response models (for example
/// <see cref="AccountStatusWire"/>); this only decides how each value appears in the result.
/// </summary>
internal static class AccountsMapper
{
    /// <summary>Maps a <c>SW01</c> account status response.</summary>
    internal static AccountStatusResult ToAccountStatusResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.AccountStatusWire, envelope.HttpStatusCode);

        return new AccountStatusResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            wire.Balance,
            wire.AccountName ?? string.Empty,
            wire.IsActivated,
            wire.CallbackUrl);
    }

    /// <summary>Maps a create (<c>WC02</c>) or update (<c>WCU02</c>) response, whose <c>data</c> is the wallet.</summary>
    internal static AccountResult ToAccountResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.AccountWire, envelope.HttpStatusCode);

        var details = new AccountDetails(
            wire.IsEnabled,
            wire.AccountNumber ?? string.Empty,
            wire.AccountName ?? string.Empty,
            wire.PaymentId ?? string.Empty,
            wire.ApiEnabled,
            // Moolre sends "" when no callback is set; the URL converter turns that into null.
            wire.CallbackUrl,
            // Moolre sends {} when settlement is not configured; empty values read as null.
            wire.Settlement ?? new SettlementSettings(),
            // The callback secret. Sensitive: the SDK never logs it.
            wire.Secret ?? string.Empty);

        return new AccountResult(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, details);
    }

    /// <summary>
    /// Maps a transaction list. <c>ST08</c> carries <c>txcount</c> and a <c>transactions</c> array; <c>ST09</c> (none found) has no
    /// documented body, so anything missing reads as an empty list.
    /// </summary>
    internal static AccountTransactionsResult ToAccountTransactionsResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.AccountTransactionsWire, envelope.HttpStatusCode);
        var transactions = wire.Transactions.Select(t => t.ToTransaction()).ToList();

        // txcount is Moolre's total; the number of transactions received fills in when it is missing.
        return new AccountTransactionsResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            wire.TransactionCount ?? transactions.Count,
            transactions);
    }
}
