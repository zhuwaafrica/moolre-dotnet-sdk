namespace Zhuwa.Moolre.Transfers;

/// <summary>
/// Sends money from your wallet to mobile money, banks and other Moolre wallets, and checks names and transfer status.
/// Calls that move money are never retried by the SDK; if the outcome is unknown you get <see cref="MoolreOutcomeUncertainException"/>.
/// </summary>
public abstract class MoolreTransfers
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreTransfers()
    {
    }

    /// <summary>
    /// Looks up the name on a mobile money or bank account before you send money to it. Safe to repeat.
    /// A number that is not found is a normal result (<see cref="NameValidationResult.Found"/> is <see langword="false"/>), not an exception.
    /// </summary>
    /// <param name="request">The account to check.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The account holder's name, or a not-found result.</returns>
    /// <exception cref="ArgumentException">A required value is missing, or the bank code is missing for a bank account.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or a credential is missing.</exception>
    /// <exception cref="MoolreAuthenticationException">Moolre rejected the credentials.</exception>
    /// <exception cref="MoolreApiException">Moolre reported another failure.</exception>
    public virtual Task<NameValidationResult> ValidateNameAsync(ValidateNameRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Sends money to a mobile money number or a bank account. <b>Moves money</b>: never retried. Use a new, unique
    /// <see cref="SendTransferRequest.ExternalReference"/> for each transfer, and keep it.
    /// </summary>
    /// <param name="request">The transfer.</param>
    /// <param name="cancellationToken">
    /// Cancels the call. Cancelling after the transfer started gives <see cref="MoolreOutcomeUncertainException"/>, because it may have gone through.
    /// </param>
    /// <returns>
    /// The transfer as Moolre reports it. Check <see cref="TransferResult.Status"/>: <see cref="TransactionStatus.Pending"/> means
    /// confirm later with <see cref="GetStatusAsync"/>.
    /// </returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid (for example an amount that is not above zero).</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used. Do not resend; check the original.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The transfer may or may not have happened. Check its status with the same reference.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the transfer (for example insufficient balance). No money moved.</exception>
    public virtual Task<TransferResult> SendAsync(SendTransferRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the current state of a transfer, by your external reference or by Moolre's ID. Safe to repeat.</summary>
    /// <param name="request">Which transfer.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The transaction.</returns>
    /// <exception cref="ArgumentException">The ID is missing or the ID kind is not set.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or a credential is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<TransactionResult> GetStatusAsync(GetTransferStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Moves money from your wallet to another Moolre wallet. <b>Moves money</b>: never retried. Moolre only confirms that the transfer
    /// started; check the final state with <see cref="GetStatusAsync"/>.
    /// </summary>
    /// <param name="request">The transfer.</param>
    /// <param name="cancellationToken">
    /// Cancels the call. Cancelling after the transfer started gives <see cref="MoolreOutcomeUncertainException"/>.
    /// </param>
    /// <returns>The ID Moolre gave the transfer.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the private key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The transfer may or may not have happened.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the transfer (for example insufficient balance). No money moved.</exception>
    public virtual Task<InternalTransferResult> SendInternalAsync(SendInternalTransferRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
