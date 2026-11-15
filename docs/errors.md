# Errors and money safety

## The exceptions

All SDK exceptions derive from `MoolreException`.

| Exception | Meaning | What to do |
| --- | --- | --- |
| `MoolreAuthenticationException` | Moolre rejected the credentials, the wallet was not found, or API access is not activated (`AIN01`, `AIN03`, `AIN04`, `IN01`, `ASL00`, `ASS00`). | Fix the configuration. Retrying will not help. |
| `MoolreDuplicateReferenceException` | The external reference was already used (`TP13`, `TR013`, `APY18`, `ASL08`, `INP02`). | Do **not** resend with a new reference. Look up the original with its status call. |
| `MoolreApiException` | Moolre refused the call (status 0), for example insufficient balance. For money calls, nothing moved. | Read `Code`, `Message` and `Detail`; show the user or fix the request. |
| `MoolreOutcomeUncertainException` | A call that moves money or sends a message timed out, lost its connection, was cancelled after it started, or got an unreadable answer. **It may or may not have happened.** | Follow the recipe below. Never resend blindly. |
| `MoolreResponseException` | Moolre's answer could not be read (not JSON, not a Moolre response). | For read-only calls, try again later. |

Standard .NET exceptions are used too:

- `ArgumentException` (and `ArgumentNullException`, `ArgumentOutOfRangeException`) for a bad request, thrown **before anything is sent**: a missing value, an amount that is not above zero or has more than two decimals, a sender ID over 11 characters, a URL that is not `http` or `https`.
- `InvalidOperationException` for missing configuration, for example no account number or no key for the call. Also thrown before anything is sent.
- `TimeoutException` when a **read-only** call times out (it is safe to repeat), and `HttpRequestException` when a read-only call loses its connection.
- `OperationCanceledException` when you cancel a call before it was sent, or cancel a read-only call.

`MoolreApiException` (and its two subclasses) carry `Code` (Moolre's code, for example `APY21`), `HttpStatusCode`, `Messages` and `Detail` (Moolre's `data` when it is text, for example `"senderid"`).

## Valid answers are results, not exceptions

Some Moolre answers look like failures but are normal results. The SDK returns them so you can branch on them:

| Answer | Result |
| --- | --- |
| Name not found (`AVD02`) | `NameValidationResult.Found = false` |
| One-time code needed (`TP14`) | `MobileMoneyResult.Outcome = OtpRequired` |
| Payment ID or bank account not found (`AC03`) | `PaymentAccountLookupResult.Found = false` |
| Transfer in progress (`OTGH02`) | `TransferResult.Status = Pending` |
| Sale in progress (`ASL11`) | `SaleResult.IsInProgress = true` |
| Sale pending, refunded or not found (`ASS02`, `ASS03`, `ASS04`) | `SaleStatusResult.Status` |
| No transactions (`ST09`) | an empty list |
| Some WhatsApp references not unique | `WhatsAppSendResult.AllSent = false`, with `NotSent` |

## After an unknown outcome

```csharp
try
{
    await moolre.Transfers.SendAsync(request, cancellationToken);
}
catch (MoolreOutcomeUncertainException ex)
{
    // ex.ExternalReference is the reference you sent (null for bulk SMS/WhatsApp; use the messages' own references).
    // ex.Operation is, for example, "Transfers.SendAsync".
    await MarkAsNeedsCheckingAsync(ex.ExternalReference);
}
```

Then, later (a background job is a good place):

1. Look the call up with its status method and the **same** reference: `Transfers.GetStatusAsync`, `Payments.GetStatusAsync`, `Sales.GetStatusAsync`, or the SMS/WhatsApp `GetStatusAsync` with the message references.
2. If it happened, record it as done. If it was not found, it did not happen, and you may send it again with the **same** reference (a duplicate reference error then tells you it did happen after all).
3. Decide only once the status is final. A pending status means wait and check again.

## Retries

The SDK never retries a call itself. Every Moolre call is a `POST`, including payments and transfers, so a blind retry can charge a customer or send money twice.

- Read-only calls (status lookups, lists, balances, the name check) are safe to repeat; you may retry them after a `TimeoutException` or `HttpRequestException`.
- Calls that move money or send messages must be resolved with the recipe above, never retried.
- Do not add a retry policy (for example a resilience handler) to the SDK's `HttpClient`.

## What the SDK logs

The SDK logs the operation name, the HTTP status and Moolre's code, at `Debug` level (timeouts and connection failures at `Warning`). It never logs credentials, callback secrets, request bodies or response bodies.
