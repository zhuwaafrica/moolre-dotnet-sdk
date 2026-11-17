using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sales;

/// <summary>Turns Moolre's sales responses into the public result types. Field names live on the response models (for example <see cref="SaleWire"/>).</summary>
internal static class SalesMapper
{
    /// <summary>Maps an airtime or data sale response (<c>ASL11</c> in progress, or another success code).</summary>
    /// <param name="envelope">The parsed response.</param>
    /// <param name="requestedReceiver">The receiver sent, used when Moolre does not echo it back.</param>
    /// <param name="requestedReference">The reference sent, used when Moolre does not echo it back.</param>
    internal static SaleResult ToSaleResult(this ResponseEnvelope envelope, string requestedReceiver, string requestedReference)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.SaleWire, envelope.HttpStatusCode);

        // "In progress" is not delivered, whatever txstatus says; a missing status also reads as pending, never as delivered.
        var status = envelope.Code == MoolreResponseCode.SaleInProgress ? TransactionStatus.Pending : wire.Status;

        return new SaleResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            status,
            wire.Receiver ?? requestedReceiver,
            wire.TransactionId ?? string.Empty,
            wire.ExternalReference ?? requestedReference,
            wire.Amount);
    }

    /// <summary>Maps a bundle list. Each network is its own list; a network that is missing or empty gives an empty list.</summary>
    internal static BundleCatalogResult ToBundleCatalogResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.BundleCatalogWire, envelope.HttpStatusCode);

        return new BundleCatalogResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            ToBundles(wire.Mtn),
            ToBundles(wire.Telecel),
            ToBundles(wire.At));
    }

    /// <summary>
    /// Maps a sale status response. The status comes from Moolre's code (<c>ASS01</c> sent, <c>ASS02</c> failed and refunded, <c>ASS03</c>
    /// pending, <c>ASS04</c> not found); with any other code it falls back to <c>txstatus</c>, and to pending when that is missing too.
    /// </summary>
    internal static SaleStatusResult ToSaleStatusResult(this ResponseEnvelope envelope)
    {
        var wire = MoolreJson.ReadData(envelope.Data, MoolreJsonContext.Default.SaleStatusWire, envelope.HttpStatusCode);

        return new SaleStatusResult(
            envelope.Code,
            envelope.Message,
            envelope.Messages,
            envelope.HttpStatusCode,
            ToSaleStatus(envelope.Code, wire.Status),
            wire.Product,
            wire.Network,
            wire.BundleId,
            wire.Receiver,
            wire.Amount,
            wire.Discount,
            wire.Charged,
            wire.AccountNumber,
            wire.TransactionId,
            wire.RefundId,
            wire.SaleReference,
            wire.ExternalReference,
            wire.ThirdPartyReference,
            // An unreadable timestamp reads as the default value; the result reports it as "not known".
            wire.CreatedAt is { } created && created != default ? created : null,
            wire.UpdatedAt is { } updated && updated != default ? updated : null);
    }

    /// <summary>The codes a sale status lookup answers with as a normal result, even when Moolre marks them with status 0.</summary>
    internal static bool IsSaleStatusAnswer(MoolreResponseCode code) =>
        code == MoolreResponseCode.SaleSent
        || code == MoolreResponseCode.SaleFailedAndRefunded
        || code == MoolreResponseCode.SalePending
        || code == MoolreResponseCode.SaleNotFound;

    private static SaleStatus ToSaleStatus(MoolreResponseCode code, int? txStatus)
    {
        if (code == MoolreResponseCode.SaleSent)
        {
            return SaleStatus.Sent;
        }

        if (code == MoolreResponseCode.SaleFailedAndRefunded)
        {
            return SaleStatus.FailedAndRefunded;
        }

        if (code == MoolreResponseCode.SaleNotFound)
        {
            return SaleStatus.NotFound;
        }

        // ASS03, or a code the SDK does not know: only an explicit txstatus of 1 or 2 is trusted; everything else stays pending.
        return code != MoolreResponseCode.SalePending && txStatus is 1 ? SaleStatus.Sent
            : code != MoolreResponseCode.SalePending && txStatus is 2 ? SaleStatus.FailedAndRefunded
            : SaleStatus.Pending;
    }

    private static List<DataBundle> ToBundles(List<BundleWire> bundles) =>
        bundles.Select(b => new DataBundle(
            b.BundleId ?? string.Empty,
            b.Price,
            b.Label ?? string.Empty,
            b.Megabytes,
            b.Validity ?? string.Empty,
            b.Category ?? string.Empty)).ToList();
}
