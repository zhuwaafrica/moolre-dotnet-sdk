using System.Text.Json;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Reads Moolre's shared <c>Transaction</c> record. It is shared plumbing, not part of one area, because transfer status, payment
/// status, account transactions and Payment ID lookups all return the same record.
/// </summary>
internal static class TransactionMapper
{
    /// <summary>Maps an <c>SS01</c> status response, whose <c>data</c> is one transaction.</summary>
    internal static TransactionResult ToTransactionResult(this ResponseEnvelope envelope) =>
        new(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, envelope.Data.ToTransaction());

    /// <summary>Maps one transaction object. Missing fields read as defaults; a missing status reads as pending, never successful.</summary>
    internal static MoolreTransaction ToTransaction(this JsonElement data) =>
        new(
            data.ReadTransactionStatus("txstatus"),
            data.ReadInt32("txtype"),
            data.ReadString("accountnumber") ?? string.Empty,
            data.ReadString("payer") ?? string.Empty,
            data.ReadString("payee") ?? string.Empty,
            data.ReadDecimal("amount"),
            data.ReadDecimal("value"),
            // Only Payment ID and bank account lookups send fees; null means "not reported", not zero.
            data.ReadOptionalDecimal("fee"),
            data.ReadOptionalDecimal("networkfee"),
            data.ReadString("transactionid") ?? string.Empty,
            data.ReadString("externalref") ?? string.Empty,
            data.ReadString("thirdpartyref") ?? string.Empty,
            data.ReadTimestamp("ts"));
}
