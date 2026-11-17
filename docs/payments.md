# Payments

`moolre.Payments` takes money from customers. It uses your **public key**.

> A prompt, a link or a callback is not proof of payment. Before you fulfil an order, confirm the payment with `GetStatusAsync` (or a verified [webhook](webhooks.md) followed by `GetStatusAsync` for high-value orders).

## Mobile money collection

Moolre sends a prompt to the customer's phone; the customer approves it there.

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Payments;

var result = await moolre.Payments.RequestMobileMoneyAsync(new RequestMobileMoneyRequest
{
    Channel = PaymentChannel.Mtn,          // Mtn, Telecel or At
    Currency = Currency.Ghs,
    PayerPhoneNumber = "0241234567",       // local format
    Amount = 25.50m,                       // above zero, at most two decimals
    ExternalReference = order.Id,          // unique per payment; save it with the order first
}, cancellationToken);

switch (result.Outcome)
{
    case MobileMoneyOutcome.PromptSent:
        // Wait for the webhook, or poll GetStatusAsync. result.RequestId is Moolre's ID for the prompt.
        break;

    case MobileMoneyOutcome.OtpRequired:
        // Moolre sent the customer a one-time code by SMS. Ask for it, then send the same payment again with the code
        // and the SAME ExternalReference (see "Answering the one-time code" below).
        break;

    case MobileMoneyOutcome.None:
        // A code the SDK does not know. Check result.Code and the payment status before doing anything else.
        break;
}
```

| What happens | What you get |
| --- | --- |
| Prompt sent (`TR099`) | `Outcome = PromptSent` |
| One-time code needed (`TP14`) | `Outcome = OtpRequired` |
| Reference already used (`TP13`) | `MoolreDuplicateReferenceException`: do not resend; check the original |
| Moolre refuses (for example `TP01`, invalid currency) | `MoolreApiException`: nothing was charged |
| Timeout, dropped connection, unreadable answer | `MoolreOutcomeUncertainException` with your reference: check the status, do not resend |

### Answering the one-time code

```csharp
var withCode = new RequestMobileMoneyRequest
{
    Channel = PaymentChannel.Mtn,
    Currency = Currency.Ghs,
    PayerPhoneNumber = "0241234567",
    Amount = 25.50m,
    ExternalReference = order.Id,          // the same reference as the first request
    OtpCode = codeFromCustomer,
};

var retried = await moolre.Payments.RequestMobileMoneyAsync(withCode, cancellationToken);
```

Never retried by the SDK. `SkipOtp = true` skips the code step **in the sandbox only**; the SDK refuses it in live.

## Payment status

```csharp
var status = await moolre.Payments.GetStatusAsync(new GetPaymentStatusRequest
{
    IdKind = TransactionIdKind.ExternalReference,   // or MoolreId
    Id = order.Id,
}, cancellationToken);

if (status.Transaction.Status == TransactionStatus.Successful)
{
    // Money received: fulfil the order exactly once.
}
```

`Transaction.Status` is `Successful`, `Failed` or `Pending`. A status Moolre does not send, or sends in a form the SDK does not know, reads as `Pending`, never as `Successful`.

## Payment links

```csharp
var link = new CreatePaymentLinkRequest
{
    Amount = 100m,
    Email = "shop@example.com",              // your business email
    ExternalReference = order.Id,
    Currency = Currency.Ghs,
    CallbackUrl = new Uri("https://shop.example.com/moolre/callback"),   // optional
    RedirectUrl = new Uri("https://shop.example.com/thanks"),            // optional
    IsReusable = false,
    ExpiresAfter = TimeSpan.FromMinutes(30),                            // optional; whole minutes, at least one
};
link.Metadata["orderId"] = order.Id;                                   // returned to you in the callback

var created = await moolre.Payments.CreatePaymentLinkAsync(link, cancellationToken);
// Send the customer to created.AuthorizationUrl
```

URLs must be absolute `http` or `https` addresses. An expiry that is not whole minutes is refused rather than rounded.

## Payment IDs

A Payment ID is a short number a customer dials to pay you.

```csharp
var paymentId = await moolre.Payments.CreatePaymentIdAsync(new CreatePaymentIdRequest
{
    PhoneNumber = "+233267606822",     // international format for this call
    Name = "Kofi Adeniyi",
    Currency = Currency.Ghs,
    Amount = 20m,                      // optional preset amount
    ExternalReference = "PID-1001",
}, cancellationToken);

Console.WriteLine(paymentId.PaymentId);
```

## Bank collections

Create a bank account number that customers can pay into:

```csharp
var account = await moolre.Payments.CreateBankAccountAsync(new CreateBankAccountRequest
{
    Currency = Currency.Ghs,
    FirstName = "Kofi",
    LastName = "Adeniyi",
    PhoneNumber = "0244000000",
    Email = "kofi@example.com",
    ExternalReference = "BANK-1001",
}, cancellationToken);

Console.WriteLine($"{account.BankName} {account.BankAccountNumber}");
```

List them with `ListBankAccountsAsync()`, and look one up (with its payments) by the reference you created it with:

```csharp
var lookup = await moolre.Payments.GetPaymentAccountAsync(new GetPaymentAccountRequest { ExternalReference = "BANK-1001" }, cancellationToken);

if (lookup.Found)
{
    foreach (var payment in lookup.Transactions) { /* payment.Amount, payment.Status, ... */ }
}
```

"Not found" is a normal result (`Found = false`), not an exception. The same lookup works for Payment IDs.
