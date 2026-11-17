# SMS

`moolre.Sms` sends SMS and manages sender IDs. It uses **only your SMS VAS key** (`SmsVasKey`); your wallet keys are never sent with SMS calls.

## Send SMS

```csharp
using Zhuwa.Moolre.Sms;

var request = new SendSmsRequest { SenderId = "Zhuwa" };   // an approved sender ID, at most 11 characters
request.Messages.Add(new SmsMessage
{
    Recipient = "233241234567",
    Text = "Your order has shipped.",
    Reference = "SMS-1001",              // optional; needed to check delivery later; make it unique
});

var sent = await moolre.Sms.SendAsync(request, cancellationToken);
```

`sent.Code` is `SMS01` when Moolre accepts the messages. Delivery is reported per message by `GetStatusAsync`.

- **Delivery network.** `Network` picks the network Moolre delivers over (not the recipient's network). Leave it as `Default` and Moolre uses Telecel; MTN is used only if Moolre has approved it for you. Moolre accepts this setting in live only.
- **Never retried** by the SDK, because a retry could deliver the same SMS twice. A timeout gives `MoolreOutcomeUncertainException`: check the messages' status before sending again.
- **Unapproved sender ID** (`ASMS07`) throws `MoolreApiException`; nothing was sent.

### One message with the GET form

```csharp
await moolre.Sms.SendSingleAsync(new SendSingleSmsRequest
{
    SenderId = "Zhuwa",
    Recipient = "233241234567",
    Text = "Your code is 4821",          // at most 160 characters
}, cancellationToken);
```

Moolre puts the text of this call in the URL, where web servers may log it. Prefer `SendAsync` for anything sensitive.

## Delivery status

```csharp
var lookup = new GetSmsStatusRequest();
lookup.References.Add("SMS-1001");

var status = await moolre.Sms.GetStatusAsync(lookup, cancellationToken);
foreach (var message in status.Statuses)
{
    Console.WriteLine($"{message.Reference}: {message.Status}");
}
```

`Status` is Moolre's delivery status number, passed on unchanged (Moolre does not document what each number means).

## Sender IDs

```csharp
// Ask Moolre to register sender IDs (letters and digits, at most 11 characters, not only digits).
var create = new CreateSenderIdsRequest();
create.SenderIds.Add(new SenderIdRegistration { SenderId = "Zhuwa" });
await moolre.Sms.CreateSenderIdsAsync(create, cancellationToken);

// Check one.
var one = await moolre.Sms.GetSenderIdStatusAsync(new GetSenderIdStatusRequest { SenderId = "Zhuwa" }, cancellationToken);
if (one.Approval == SenderIdApproval.Approved) { /* ready to send */ }

// List all of yours.
var all = await moolre.Sms.ListSenderIdsAsync(cancellationToken);
```

`ApproveSenderIdsAsync` approves or rejects sender IDs; it needs administrative permission on the SMS service (`ASMQ09` otherwise).

## SMS balance

```csharp
var balance = await moolre.Sms.GetBalanceAsync(cancellationToken);
Console.WriteLine(balance.Balance);
```
