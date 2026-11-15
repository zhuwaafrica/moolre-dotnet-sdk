using System.Runtime.CompilerServices;

namespace Zhuwa.Moolre.WhatsApp;

/// <summary>
/// Sends WhatsApp template messages and reads templates and delivery status, with the WhatsApp service's own key
/// (<see cref="MoolreOptions.WhatsAppVasKey"/>). Sends are never retried by the SDK, because a retry could deliver a message twice.
/// </summary>
public abstract class MoolreWhatsApp
{
    /// <summary>Creates the client. For subclasses and test doubles.</summary>
    protected MoolreWhatsApp()
    {
    }

    /// <summary>Gets one page of your WhatsApp templates. Safe to repeat.</summary>
    /// <param name="request">The cursor from the previous page, or none for the first page.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The templates on this page and the cursor for the next (<see langword="null"/> on the last page).</returns>
    /// <exception cref="InvalidOperationException">The WhatsApp key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<WhatsAppTemplatesPage> GetTemplatePageAsync(GetWhatsAppTemplatesRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the first page of your WhatsApp templates. Safe to repeat.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The first page and the cursor for the next.</returns>
    public virtual Task<WhatsAppTemplatesPage> GetTemplatePageAsync(CancellationToken cancellationToken) =>
        GetTemplatePageAsync(new GetWhatsAppTemplatesRequest(), cancellationToken);

    /// <summary>Gets the first page of your WhatsApp templates. Safe to repeat.</summary>
    /// <returns>The first page and the cursor for the next.</returns>
    public virtual Task<WhatsAppTemplatesPage> GetTemplatePageAsync() =>
        GetTemplatePageAsync(new GetWhatsAppTemplatesRequest(), CancellationToken.None);

    /// <summary>
    /// Goes through every template, page by page, fetching the next page only when you reach it (nothing is collected into one big list).
    /// </summary>
    /// <param name="cancellationToken">Cancels the walk between and during pages.</param>
    /// <returns>Every template, in Moolre's order.</returns>
    /// <exception cref="MoolreResponseException">Moolre returned a cursor it had already returned, which would loop forever.</exception>
    public virtual async IAsyncEnumerable<WhatsAppTemplate> GetAllTemplatesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Every cursor seen so far: a repeated cursor would make the walk loop forever, so it stops with an error instead.
        var seen = new HashSet<string>(StringComparer.Ordinal);
        string? cursor = null;

        do
        {
            var page = await GetTemplatePageAsync(new GetWhatsAppTemplatesRequest { Cursor = cursor }, cancellationToken).ConfigureAwait(false);
            foreach (var template in page.Templates)
            {
                yield return template;
            }

            cursor = page.NextCursor;
            if (cursor is not null && !seen.Add(cursor))
            {
                throw new MoolreResponseException("Moolre returned a templates cursor it had already returned; stopping to avoid an endless loop.");
            }
        }
        while (cursor is not null);
    }

    /// <summary>
    /// Sends a template message to one or more recipients. <b>Sends messages and spends WhatsApp credit</b>: never retried. Give each message
    /// a unique <see cref="WhatsAppMessage.Reference"/> to look up its delivery later.
    /// </summary>
    /// <param name="request">The approved template, its language, and the messages.</param>
    /// <param name="cancellationToken">Cancels the call. Cancelling after it started gives <see cref="MoolreOutcomeUncertainException"/>.</param>
    /// <returns>
    /// The result. Check <see cref="WhatsAppSendResult.AllSent"/>: Moolre accepts the call but skips messages whose reference was not unique,
    /// and lists them in <see cref="WhatsAppSendResult.NotSent"/>.
    /// </returns>
    /// <exception cref="ArgumentException">A required value is missing, or there are no messages.</exception>
    /// <exception cref="InvalidOperationException">The WhatsApp key is not set.</exception>
    /// <exception cref="MoolreOutcomeUncertainException">The messages may or may not have been sent. Check their status before resending.</exception>
    /// <exception cref="MoolreApiException">Moolre refused the send (for example <c>WAS401</c>, insufficient balance). Nothing was sent.</exception>
    public virtual Task<WhatsAppSendResult> SendAsync(SendWhatsAppMessagesRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gets the delivery status of messages by their references. Safe to repeat.</summary>
    /// <param name="request">The message references.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>One status per reference Moolre knows (for example <c>accepted</c> or <c>read</c>).</returns>
    /// <exception cref="ArgumentException">No references, or a blank one.</exception>
    /// <exception cref="InvalidOperationException">The WhatsApp key is not set.</exception>
    /// <exception cref="MoolreApiException">Moolre reported a failure.</exception>
    public virtual Task<WhatsAppStatusResult> GetStatusAsync(GetWhatsAppMessageStatusRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
