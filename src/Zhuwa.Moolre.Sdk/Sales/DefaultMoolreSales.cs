using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Sales;

/// <summary>
/// Sends the sales calls. Each method checks the request, turns it into JSON from the field names on the request type, sends it through
/// the transport and decides which answers are results and which are failures; the mapper turns the answer into the result.
/// </summary>
internal sealed class DefaultMoolreSales(MoolreTransport transport) : MoolreSales
{
    public override async Task<SaleResult> SendAirtimeAsync(SendAirtimeRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendAirtimeRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendAirtimeRequest).SetAccountNumber(accountNumber);
        var reference = request.ExternalReference.Trim();

        // The reference goes to the transport too: if the outcome becomes unknown, the exception carries it so the caller checks the
        // status instead of selling twice.
        var envelope = await transport.SendAsync(MoolreEndpoints.Sales.SendAirtime, body, reference, cancellationToken).ConfigureAwait(false);
        EnsureSaleAccepted(envelope, MoolreEndpoints.Sales.SendAirtime, reference);

        return envelope.ToSaleResult(request.ReceiverPhoneNumber.Trim(), reference);
    }

    public override async Task<SaleResult> SendDataAsync(SendDataRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(SendDataRequest));

        // No amount is sent: the bundle's own price is charged, as Moolre documents.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.SendDataRequest).SetAccountNumber(accountNumber);
        var reference = request.ExternalReference.Trim();

        var envelope = await transport.SendAsync(MoolreEndpoints.Sales.SendData, body, reference, cancellationToken).ConfigureAwait(false);
        EnsureSaleAccepted(envelope, MoolreEndpoints.Sales.SendData, reference);

        return envelope.ToSaleResult(request.ReceiverPhoneNumber.Trim(), reference);
    }

    public override async Task<BundleCatalogResult> ListBundlesAsync(ListBundlesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Network is SalesNetwork network)
        {
            _ = network.ToWire(nameof(request.Network));
        }

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(ListBundlesRequest));

        // This endpoint has no "type" field; the endpoint table leaves it out.
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.ListBundlesRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Sales.ListBundles, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);
        MoolreTransport.EnsureSuccess(envelope);

        return envelope.ToBundleCatalogResult();
    }

    public override async Task<SaleStatusResult> GetStatusAsync(GetSaleStatusRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var accountNumber = transport.ResolveAccountNumber(request.AccountNumber, nameof(GetSaleStatusRequest));
        var body = MoolreJson.ToBody(request, MoolreJsonContext.Default.GetSaleStatusRequest).SetAccountNumber(accountNumber);

        var envelope = await transport.SendAsync(MoolreEndpoints.Sales.GetStatus, body, externalReference: null, cancellationToken)
            .ConfigureAwait(false);

        // Sent, failed and refunded, pending and not found are all answers about the sale, whatever status Moolre marks them with.
        // Anything else at status 0 (for example ASS08, unknown error) means Moolre could not answer.
        if (!SalesMapper.IsSaleStatusAnswer(envelope.Code))
        {
            MoolreTransport.EnsureSuccess(envelope);
        }

        return envelope.ToSaleStatusResult();
    }

    // Decides what a sale answer means for the money. "In progress" (ASL11) is a result. ASL06 is Moolre's "unknown error, the sale may
    // still have gone through", so it is reported as an unknown outcome with the reference, never as a failure. Any other status 0 is a
    // refusal, and nothing was spent; a duplicate reference (ASL08) was already thrown by the transport.
    private static void EnsureSaleAccepted(ResponseEnvelope envelope, MoolreEndpoint endpoint, string reference)
    {
        if (envelope.Code == MoolreResponseCode.SaleOutcomeUnknown)
        {
            throw new MoolreOutcomeUncertainException(
                $"{endpoint.Name}: Moolre reported an unknown error ({envelope.Code}). The sale may have gone through; check its status before doing anything else.",
                endpoint.Name,
                reference);
        }

        if (envelope.Code != MoolreResponseCode.SaleInProgress)
        {
            MoolreTransport.EnsureSuccess(envelope);
        }
    }

    // Checks an airtime sale: network, receiver, reference and amount. Moolre enforces its GHS 1.00 minimum itself.
    private static void Validate(SendAirtimeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReceiverPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        RequestGuard.ThrowIfInvalidAmount(request.Amount);
        _ = request.Network.ToWire(nameof(request.Network));
    }

    // Checks a data sale: network, receiver, bundle ID and reference.
    private static void Validate(SendDataRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ReceiverPhoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.BundleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExternalReference);
        _ = request.Network.ToWire(nameof(request.Network));
    }

    // Checks a sale status request: an ID and its kind.
    private static void Validate(GetSaleStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        _ = request.IdKind.ToWire(nameof(request.IdKind));
    }
}
