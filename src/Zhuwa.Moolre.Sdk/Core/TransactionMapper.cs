namespace Zhuwa.Moolre.Core;

/// <summary>
/// Reads Moolre's shared <c>Transaction</c> record. It is shared plumbing, not part of one area, because transfer status, payment
/// status, account transactions and Payment ID lookups all return the same record.
/// </summary>
internal static class TransactionMapper
{
    /// <summary>Maps an <c>SS01</c> status response, whose <c>data</c> is one transaction.</summary>
    internal static TransactionResult ToTransactionResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.TransactionWire, envelope.HttpStatusCode);
        return new(envelope.Code, envelope.Message, envelope.Messages, envelope.HttpStatusCode, wire.ToTransaction());
    }

    /// <summary>Maps one transaction. A missing status reads as pending, never successful (see <see cref="TransactionStatusConverter"/>).</summary>
    internal static MoolreTransaction ToTransaction(this TransactionWire wire) =>
        new(
            wire.Status,
            wire.TransactionType,
            wire.AccountNumber ?? string.Empty,
            wire.Payer ?? string.Empty,
            wire.Payee ?? string.Empty,
            wire.Amount,
            wire.Value,
            // Only Payment ID and bank account lookups send fees; null means "not reported", not zero.
            wire.Fee,
            wire.NetworkFee,
            wire.TransactionId ?? string.Empty,
            wire.ExternalReference ?? string.Empty,
            wire.ThirdPartyReference ?? string.Empty,
            wire.Timestamp);
}
