namespace Zhuwa.Moolre.Sales;

/// <summary>
/// Sells airtime and data bundles from your wallet (needs Moolre's "Sell on Our Platform" service linked to the wallet), lists bundles
/// and checks sales. Sales are never retried by the SDK; a sale that comes back "in progress" is not delivered yet, so check its status.
/// </summary>
public abstract class MoolreSales
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreSales()
    {
    }

    /// <summary>
    /// Sends airtime to a phone. <b>Spends money from the wallet</b>: never retried. Use a new, unique
    /// <see cref="SendAirtimeRequest.ExternalReference"/> for each sale, and keep it.
    /// </summary>
    /// <param name="request">The sale. Moolre's minimum is GHS 1.00.</param>
    /// <param name="cancellationToken">Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <returns>The sale. <see cref="SaleResult.IsInProgress"/> means not delivered yet; confirm with <see cref="GetStatusAsync"/>.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The sale may or may not have gone through (including Moolre's <c>ASL06</c>, unknown error). Check its status.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the sale (for example an amount under GHS 1.00). Nothing was spent.</exception>
    public virtual Task<SaleResult> SendAirtimeAsync(SendAirtimeRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Sends a data bundle to a phone. <b>Spends money from the wallet</b>: never retried. The bundle's own price is charged, so no amount
    /// is sent. Use a new, unique <see cref="SendDataRequest.ExternalReference"/> for each sale.
    /// </summary>
    /// <param name="request">The sale; the bundle ID comes from <see cref="ListBundlesAsync(ListBundlesRequest, CancellationToken)"/>.</param>
    /// <param name="cancellationToken">Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <returns>The sale. <see cref="SaleResult.IsInProgress"/> means not delivered yet; confirm with <see cref="GetStatusAsync"/>.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The sale may or may not have gone through. Check its status.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the sale (for example a bundle not sold on that network). Nothing was spent.</exception>
    public virtual Task<SaleResult> SendDataAsync(SendDataRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists the data bundles Moolre sells, per network. Safe to repeat.</summary>
    /// <param name="request">The wallet and an optional network; all networks when the network is omitted.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The bundles for MTN, Telecel and AT (an empty list for a network with none).</returns>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<BundleCatalogResult> ListBundlesAsync(ListBundlesRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists the data bundles for every network, for the default wallet. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The bundles for MTN, Telecel and AT.</returns>
    public virtual Task<BundleCatalogResult> ListBundlesAsync(CancellationToken cancellationToken) =>
        ListBundlesAsync(new ListBundlesRequest(), cancellationToken);

    /// <summary>Lists the data bundles for every network, for the default wallet. Safe to repeat.</summary>
    /// <returns>The bundles for MTN, Telecel and AT.</returns>
    public virtual Task<BundleCatalogResult> ListBundlesAsync() =>
        ListBundlesAsync(new ListBundlesRequest(), CancellationToken.None);

    /// <summary>
    /// Gets where a sale stands: sent, pending, failed and refunded, or not found. Safe to repeat. Every one of these is a normal result.
    /// </summary>
    /// <param name="request">Which sale, by your external reference, Moolre's transaction ID or the sale reference.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The sale's status and details.</returns>
    /// <exception cref="ArgumentException">The ID is missing or the ID kind is not set.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre could not answer (for example <c>ASS08</c>, unknown error). Do not assume the sale failed.</exception>
    public virtual Task<SaleStatusResult> GetStatusAsync(GetSaleStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
