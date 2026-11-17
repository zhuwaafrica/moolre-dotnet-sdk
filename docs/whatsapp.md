# WhatsApp

`moolre.WhatsApp` sends WhatsApp template messages. It uses **only your WhatsApp VAS key** (`WhatsAppVasKey`), which is a different key from SMS.

## Templates

Every WhatsApp message uses a template approved in Moolre. Walk all of them, page by page:

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.WhatsApp;

await foreach (var template in moolre.WhatsApp.GetAllTemplatesAsync(cancellationToken))
{
    Console.WriteLine($"{template.Name} ({template.Status}): {template.Body}");
}
```

The next page is fetched only when you reach it, so nothing is collected into one big list. If Moolre ever returned the same page cursor twice, the walk stops with `MoolreResponseException` instead of looping forever. To read one page at a time, use `GetTemplatePageAsync` and pass `NextCursor` back as `Cursor`.

`template.Status` is `WhatsAppTemplateStatus.Approved`, `Pending` or `Rejected` (or a value Moolre adds later, kept exactly as text). Only approved templates can be sent.

## Send messages

Placeholders fill the template's `{{1}}`, `{{2}}`, ... in order:

```csharp
var request = new SendWhatsAppMessagesRequest { TemplateName = "promotion", Language = "en" };

var message = new WhatsAppMessage { Recipient = "233531419000", Reference = "WA-1001" };   // reference: unique, to check delivery later
message.Placeholders.Add("Ama");      // {{1}}
message.Placeholders.Add("loans");    // {{2}}
message.Placeholders.Add("Zhuwa");    // {{3}}
request.Messages.Add(message);

var result = await moolre.WhatsApp.SendAsync(request, cancellationToken);

if (!result.AllSent)
{
    // Moolre still answered "success" but skipped messages whose reference was not unique.
    Console.WriteLine(result.Warning);
    foreach (var skipped in result.NotSent) { /* skipped.Recipient, skipped.Reference */ }
}
```

- **Never retried** by the SDK. A timeout gives `MoolreOutcomeUncertainException`: check the messages' status before sending again.
- **Insufficient WhatsApp balance** (`WAS401`) throws `MoolreApiException`; nothing was sent.

## Delivery status

```csharp
var lookup = new GetWhatsAppMessageStatusRequest();
lookup.References.Add("WA-1001");

var status = await moolre.WhatsApp.GetStatusAsync(lookup, cancellationToken);
foreach (var item in status.Statuses)
{
    if (item.Status == WhatsAppDeliveryStatus.Read) { /* the recipient read it */ }
}
```

Statuses are kept exactly as Moolre sends them (for example `accepted`, `read`).
