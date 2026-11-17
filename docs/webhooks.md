# Payment webhooks

When a payment succeeds or fails, Moolre sends a `POST` to your callback URL. The SDK reads the body and checks Moolre's signature; what you do with it stays in your code.

## Register the verifier

```csharp
using Zhuwa.Moolre.DependencyInjection;

builder.Services.AddMoolreWebhooks(o => o.Secret = builder.Configuration["Moolre:CallbackSecret"]);
```

The callback secret belongs to your wallet: it is returned as `Account.Secret` when you create or update a wallet, and shown in the Moolre app. The app refuses to start without one.

## How Moolre signs callbacks

Every callback carries an `X-Moolre-Signature` header (`MoolreWebhookVerifier.SignatureHeaderName`):

```text
X-Moolre-Signature: t=1763200000,v1=4bf12547ee6075cd56a2ef3803346428fd758cf3c630297e6012dec22e5189cd
```

`v1` is the HMAC-SHA256 of `"<t>.<raw body>"`, keyed with your wallet's callback secret. `IsSignatureValid` recomputes it, compares in constant time, and rejects a callback whose `t` is more than 5 minutes from your clock (`SignatureTolerance`), so a captured request cannot be replayed. Pass the body **exactly as received**: re-serialised JSON has different bytes and never matches.

## Handle the callback (ASP.NET Core)

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Webhooks;

app.MapPost("/moolre/callback", async (HttpRequest http, MoolreWebhookVerifier verifier, OrderService orders) =>
{
    using var body = new MemoryStream();
    await http.Body.CopyToAsync(body);
    var rawBody = body.ToArray();

    // 1. Is it really from Moolre? Check the signature on the raw bytes before reading anything.
    if (!verifier.IsSignatureValid(rawBody, http.Headers[MoolreWebhookVerifier.SignatureHeaderName]))
    {
        return Results.Unauthorized();
    }

    // 2. Is it a payment callback?
    if (!PaymentWebhook.TryParse(rawBody, out var webhook))
    {
        return Results.BadRequest();
    }

    // 3. Moolre retries until you answer 2xx, with the same event ID every time: handle each event once.
    if (await orders.AlreadyProcessedAsync(webhook.EventId ?? webhook.Data.ExternalReference))
    {
        return Results.Ok();
    }

    // 4. Only a full success counts: status 1, code P01 and transaction status 1.
    if (webhook.IsSuccessfulPayment)
    {
        await orders.ConfirmAndFulfilAsync(webhook.Data.ExternalReference);   // confirm with GetStatusAsync for high-value orders
    }
    else
    {
        await orders.RecordFailureAsync(webhook.Data.ExternalReference, webhook.Data.Reason);   // never fulfil from a failed callback
    }

    return Results.Ok();
});
```

`webhook.EventId` (`event_id`) and `webhook.EventType` (`event`, such as `payment.succeeded`) are null on callbacks sent without event fields. `webhook.Data` has the payer, amount, value after fees, Moolre's transaction ID (null on failed attempts), your external reference, the failure reason and the timestamp (GMT, as UTC).

## Several wallets at one endpoint

Each wallet has its own callback secret. Key them by the wallet's account number; each callback is checked against the secret of the wallet in its `data.accountnumber`:

```csharp
builder.Services.AddMoolreWebhooks(o =>
{
    o.WalletSecrets["10772306069982"] = builder.Configuration["Moolre:Collections:CallbackSecret"]!;
    o.WalletSecrets["10927206072742"] = builder.Configuration["Moolre:Payouts:CallbackSecret"]!;
});
```

A callback signed with another wallet's secret, or for a wallet with no secret configured, is rejected. `Secret` can be set as well, as a fallback for wallets not in the list and for events that name no wallet (such as `webhook.test`).

## The old `data.secret` check

Callbacks also still carry the secret itself in `data.secret`. Moolre has deprecated it and will remove it after 31 March 2027, so `IsSecretValid` and `PaymentWebhookData.Secret` are marked `[Obsolete]`. Move to `IsSignatureValid`; the same secrets work for both.

## Moolre's callback addresses

Moolre documents the addresses it sends callbacks from. You can allow only these at your firewall, load balancer or reverse proxy, as an extra layer on top of the signature check (not instead of it). Moolre can change them, so check Moolre's documentation:

| IPv4 | IPv6 |
| --- | --- |
| `174.138.44.22` | `2604:a880:400:d1:0:3:4cf0:c001` |
| `174.138.40.67` | `2604:a880:400:d0::1a77:4001` |
| `192.241.135.134` | `2604:a880:400:d0::4234:2001` |

(As listed on Moolre's payment webhook page, read 14 November 2026.)
