namespace Zhuwa.Moolre.Payments;

/// <summary>
/// Takes payments: mobile money prompts, payment links, Payment IDs and bank account numbers for collections, and payment status.
/// Calls that take money are never retried by the SDK; if the outcome is unknown you get <see cref="MoolreOutcomeUncertainException"/>.
/// A prompt or a link is not proof of payment: confirm with <see cref="GetStatusAsync"/> or the payment webhook before fulfilling an order.
/// </summary>
public abstract class MoolrePayments
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolrePayments()
    {
    }

    /// <summary>
    /// Asks the customer to approve a mobile money payment on their phone. <b>Takes money</b>: never retried. Use a new, unique
    /// <see cref="RequestMobileMoneyRequest.ExternalReference"/> for each payment, and keep it.
    /// </summary>
    /// <param name="request">The payment.</param>
    /// <param name="cancellationToken">
    /// Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>, because the prompt may have been sent.
    /// </param>
    /// <returns>
    /// What happened: <see cref="MobileMoneyOutcome.PromptSent"/> (wait for the webhook or check the status) or
    /// <see cref="MobileMoneyOutcome.OtpRequired"/> (send the same request again with the customer's code in <c>OtpCode</c>).
    /// </returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid, or <c>SkipOtp</c> is set outside the sandbox.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used. Do not resend; check the original.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The prompt may or may not have been sent. Check the status with the same reference.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the request (for example an unsupported currency). Nothing was charged.</exception>
    public virtual Task<MobileMoneyResult> RequestMobileMoneyAsync(RequestMobileMoneyRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the current state of a payment, by your external reference or by Moolre's ID. Safe to repeat.</summary>
    /// <param name="request">Which payment.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The transaction. Only <see cref="TransactionStatus.Successful"/> means the money arrived.</returns>
    /// <exception cref="ArgumentException">The ID is missing or the ID kind is not set.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<TransactionResult> GetStatusAsync(GetPaymentStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Creates a hosted checkout link to send to the customer. Not retried automatically.</summary>
    /// <param name="request">The link.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The link and Moolre's reference for it.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid (for example an expiry under one minute).</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreDuplicateReferenceException">The external reference was already used.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The link may or may not have been created.</exception>
    /// <exception cref="MoolreResponseException">Moolre reported success but sent no usable link.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the request.</exception>
    public virtual Task<PaymentLinkResult> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Creates a Payment ID that a customer dials to pay you. Not retried automatically.</summary>
    /// <param name="request">The Payment ID.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Payment ID.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid (for example a phone number not in +233 format).</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The Payment ID may or may not have been created.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the request.</exception>
    public virtual Task<PaymentIdResult> CreatePaymentIdAsync(CreatePaymentIdRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Creates a bank account number that customers pay into. Not retried automatically.</summary>
    /// <param name="request">The account holder.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The new account number, name and bank.</returns>
    /// <exception cref="ArgumentException">A required value is missing or invalid.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The account may or may not have been created.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the request (for example <c>AD32</c>, the account could not be created).</exception>
    public virtual Task<BankAccountResult> CreateBankAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists the bank account numbers created for a wallet. Safe to repeat.</summary>
    /// <param name="request">Which wallet; the default from <see cref="MoolreOptions.AccountNumber"/> when its account number is omitted.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The accounts.</returns>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<BankAccountListResult> ListBankAccountsAsync(ListBankAccountsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists the bank account numbers created for the default wallet. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The accounts.</returns>
    public virtual Task<BankAccountListResult> ListBankAccountsAsync(CancellationToken cancellationToken) =>
        ListBankAccountsAsync(new ListBankAccountsRequest(), cancellationToken);

    /// <summary>Lists the bank account numbers created for the default wallet. Safe to repeat.</summary>
    /// <returns>The accounts.</returns>
    public virtual Task<BankAccountListResult> ListBankAccountsAsync() =>
        ListBankAccountsAsync(new ListBankAccountsRequest(), CancellationToken.None);

    /// <summary>
    /// Looks up a Payment ID or bank account by the reference used to create it, with its payments. Safe to repeat.
    /// Not found is a normal result (<see cref="PaymentAccountLookupResult.Found"/> is <see langword="false"/>), not an exception.
    /// </summary>
    /// <param name="request">Which Payment ID or bank account.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The account summary and its transactions, or a not-found result.</returns>
    /// <exception cref="ArgumentException">The external reference is missing.</exception>
    /// <exception cref="InvalidOperationException">No account number is set on the request or in the options, or the public key is missing.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<PaymentAccountLookupResult> GetPaymentAccountAsync(GetPaymentAccountRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
