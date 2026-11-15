# Payment webhooks

When a payment succeeds or fails, Moolre sends a `POST` to your callback URL. The SDK reads the body and checks the callback secret; what you do with it stays in your code.

## Register the verifier

```csharp
using Zhuwa.Moolre.DependencyInjection;

builder.Services.AddMoolreWebhooks(o => o.Secret = builder.Configuration["Moolre:CallbackSecret"]);
```

The callback secret belongs to your wallet: it is returned as `Account.Secret` when you create or update a wallet, and shown in the Moolre app. The app refuses to start without one.

## Handle the callback (ASP.NET Core)

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Webhooks;

app.MapPost("/moolre/callback", async (HttpRequest http, MoolreWebhookVerifier verifier, OrderService orders) =>
{
    using var body = new MemoryStream();
    await http.Body.CopyToAsync(body);

    // 1. Is it a Moolre callback at all?
    if (!PaymentWebhook.TryParse(body.ToArray(), out var webhook))
    {
        return Results.BadRequest();
    }

    // 2. Is it really from Moolre? Check the secret before touching any record.
    if (!verifier.IsSecretValid(webhook))
    {
        return Results.Unauthorized();     // never log webhook.Data.Secret
    }

    // 3. Moolre may send the same callback more than once: handle it once.
    if (await orders.AlreadyProcessedAsync(webhook.Data.ExternalReference))
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

`webhook.Data` has the payer, amount, value after fees, Moolre's transaction ID (null on failed attempts), your external reference, the failure reason and the timestamp (GMT, as UTC).

## Several wallets at one endpoint

Each wallet has its own callback secret. Key them by the wallet's account number; each callback is checked against its own wallet's secret:

```csharp
builder.Services.AddMoolreWebhooks(o =>
{
    o.WalletSecrets["10772306069982"] = builder.Configuration["Moolre:Collections:CallbackSecret"]!;
    o.WalletSecrets["10927206072742"] = builder.Configuration["Moolre:Payouts:CallbackSecret"]!;
});
```

A callback carrying another wallet's secret, or for a wallet with no secret configured, is rejected. `Secret` can be set as well, as a fallback for wallets not in the list.

## Moolre's callback addresses

Moolre documents the addresses it sends callbacks from. You can allow only these at your firewall, load balancer or reverse proxy, as an extra layer on top of the secret check (not instead of it). Moolre can change them, so check Moolre's documentation:

| IPv4 | IPv6 |
| --- | --- |
| `174.138.44.22` | `2604:a880:400:d1:0:3:4cf0:c001` |
| `174.138.40.67` | `2604:a880:400:d0::1a77:4001` |
| `192.241.135.134` | `2604:a880:400:d0::4234:2001` |

(As listed on Moolre's payment webhook page, read 14 November 2026.)
