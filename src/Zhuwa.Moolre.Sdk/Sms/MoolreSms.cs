namespace Zhuwa.Moolre.Sms;

/// <summary>
/// Sends SMS and manages sender IDs, with the SMS service's own key (<see cref="MoolreOptions.SmsVasKey"/>). Sends are never retried by the
/// SDK, because a retry could deliver the same message twice.
/// </summary>
public abstract class MoolreSms
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreSms()
    {
    }

    /// <summary>
    /// Sends one or more SMS from an approved sender ID. <b>Sends messages and spends SMS credit</b>: never retried. Give each message a
    /// <see cref="SmsMessage.Reference"/> if you want to look up its delivery later with <see cref="GetStatusAsync"/>.
    /// </summary>
    /// <param name="request">The sender ID and the messages.</param>
    /// <param name="cancellationToken">Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <returns>Moolre's acceptance (<c>SMS01</c>). Delivery is reported per message by <see cref="GetStatusAsync"/>.</returns>
    /// <exception cref="ArgumentException">A required value is missing, a sender ID is longer than 11 characters, or there are no messages.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The messages may or may not have been sent. Check their status before resending.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the send (for example <c>ASMS07</c>, the sender ID is not approved). Nothing was sent.</exception>
    public virtual Task<SmsSendResult> SendAsync(SendSmsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Sends one SMS to one recipient with Moolre's GET form (at most 160 characters). <b>Sends a message</b>: never retried. Moolre puts the
    /// message text in the URL for this call, so it can appear in web server logs; prefer <see cref="SendAsync"/> for anything sensitive.
    /// </summary>
    /// <param name="request">The sender ID, recipient and text.</param>
    /// <param name="cancellationToken">Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <returns>Moolre's acceptance (<c>SMS01</c>).</returns>
    /// <exception cref="ArgumentException">A required value is missing, the sender ID is longer than 11 characters, or the text is longer than 160.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The message may or may not have been sent.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the send. Nothing was sent.</exception>
    public virtual Task<SmsSendResult> SendSingleAsync(SendSingleSmsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the delivery status of messages by the references you gave them. Safe to repeat.</summary>
    /// <param name="request">The message references.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One status per reference Moolre knows.</returns>
    /// <exception cref="ArgumentException">No references, or a blank one.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<SmsStatusResult> GetStatusAsync(GetSmsStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Asks Moolre to register sender IDs (at most 11 characters, letters and digits, not only digits). Not retried automatically.</summary>
    /// <param name="request">The sender IDs.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Moolre's acceptance (<c>ASMQ12</c>). Approval comes later; check it with <see cref="GetSenderIdStatusAsync"/>.</returns>
    /// <exception cref="ArgumentException">No sender IDs, a blank one, or one longer than 11 characters.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The request may or may not have reached Moolre.</exception>
    /// <exception cref="MoolreApiException">Moolre refused it (for example <c>ASMQ09</c>, no permission to approve).</exception>
    public virtual Task<SenderIdsCreatedResult> CreateSenderIdsAsync(CreateSenderIdsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets whether a sender ID is approved, pending or rejected. Safe to repeat.</summary>
    /// <param name="request">The sender ID.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The sender ID's approval and whitelisting.</returns>
    /// <exception cref="ArgumentException">The sender ID is missing.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<SenderIdStatusResult> GetSenderIdStatusAsync(GetSenderIdStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Lists your sender IDs and their approval. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The sender IDs.</returns>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<SenderIdListResult> ListSenderIdsAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <summary>Lists your sender IDs and their approval. Safe to repeat.</summary>
    /// <returns>The sender IDs.</returns>
    public virtual Task<SenderIdListResult> ListSenderIdsAsync() => ListSenderIdsAsync(CancellationToken.None);

    /// <summary>Approves, rejects or resets sender IDs. Needs administrative permission on the SMS service. Not retried automatically.</summary>
    /// <param name="request">The decisions.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The sender IDs after the change.</returns>
    /// <exception cref="ArgumentException">No decisions, or a blank sender ID.</exception>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The change may or may not have been applied.</exception>
    /// <exception cref="MoolreApiException">Moolre refused it (for example <c>ASMQ09</c>, no permission).</exception>
    public virtual Task<SenderIdListResult> ApproveSenderIdsAsync(ApproveSenderIdsRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the SMS credit balance. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The balance.</returns>
    /// <exception cref="InvalidOperationException">The SMS key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<SmsBalanceResult> GetBalanceAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <summary>Gets the SMS credit balance. Safe to repeat.</summary>
    /// <returns>The balance.</returns>
    public virtual Task<SmsBalanceResult> GetBalanceAsync() => GetBalanceAsync(CancellationToken.None);
}
