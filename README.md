# Moolre .NET SDK

An **unofficial, community-built** .NET client for the [Moolre](https://docs.moolre.com/) API, maintained by [Zhuwa Africa](https://github.com/zhuwaafrica). One NuGet package, `Zhuwa.Moolre.Sdk`, covers all 31 documented Moolre operations: mobile money collections, payment links, Payment IDs and bank collections, transfers, wallets, airtime and data, SMS and WhatsApp, plus payment webhooks.

> [!NOTE]
> This project is independent. It is not affiliated with, endorsed by, or supported by Moolre.

> [!WARNING]
> **Status: preview.** Every call is covered by unit tests against Moolre's documented responses; they have not yet been verified against the real Moolre sandbox.

**Contents:** [Requirements](#requirements) · [Installation](#installation) · [Usage](#usage) · [Several wallets](#several-wallets) · [Exception handling](#exception-handling) · [Retries and timeouts](#retries-and-timeouts) · [Webhooks](#webhooks) · [Documentation](#documentation) · [Contributing](#contributing)

## Requirements

- .NET 10 or later. The package works with trimming and Native AOT.
- Moolre API credentials. Moolre gives each **wallet** its own credentials, created in the Moolre app. Which ones you need depends on the calls you make:

| Credential | Setting | Used by |
| --- | --- | --- |
| API user | `ApiUser` | Accounts, Transfers, Payments, Sales |
| Private key | `PrivateKey` | Transfers, Sales, Accounts |
| Public key | `PublicKey` | Payments |
| SMS VAS key | `SmsVasKey` | SMS |
| WhatsApp VAS key | `WhatsAppVasKey` | WhatsApp |

## Installation

```bash
dotnet add package Zhuwa.Moolre.Sdk --prerelease
```

## Usage

Register the SDK once in `Program.cs`. Read credentials from configuration or a secret store, never source code; the settings are checked when the app starts.

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.DependencyInjection;

builder.Services.AddMoolre(options =>
{
    options.Environment = MoolreEnvironment.Sandbox;   // the default; Live is an explicit choice
    options.ApiUser = builder.Configuration["Moolre:ApiUser"];
    options.PublicKey = builder.Configuration["Moolre:PublicKey"];
    options.PrivateKey = builder.Configuration["Moolre:PrivateKey"];
    options.AccountNumber = builder.Configuration["Moolre:AccountNumber"];
});
```

Then inject `MoolreClient`. Each API area is a property:

| Property | What it does |
| --- | --- |
| `Payments` | Mobile money collection, payment status, payment links, Payment IDs, bank collections |
| `Transfers` | Name check, transfers to mobile money and banks, wallet-to-wallet transfers, transfer status |
| `Accounts` | Wallet balance and settings, transaction history, sandbox wallets |
| `Sales` | Airtime and data bundles, the bundle list, sale status |
| `Sms` | Sending SMS, delivery status, sender IDs, SMS balance |
| `WhatsApp` | Templates, template messages, delivery status |
| `ReferenceData` | The bank list |

```csharp
using Zhuwa.Moolre.Payments;

public class CheckoutService(MoolreClient moolre)
{
    public async Task StartPaymentAsync(Order order, CancellationToken cancellationToken)
    {
        var result = await moolre.Payments.RequestMobileMoneyAsync(new RequestMobileMoneyRequest
        {
            Channel = PaymentChannel.Mtn,
            Currency = Currency.Ghs,
            PayerPhoneNumber = order.Phone,     // local format, for example 0241234567
            Amount = order.Total,               // above zero, at most two decimals
            ExternalReference = order.Id,       // unique per payment; save it first
        }, cancellationToken);

        // result.Outcome is PromptSent (wait for the webhook) or OtpRequired (send again with the customer's code).
    }

    public async Task<bool> IsPaidAsync(Order order, CancellationToken cancellationToken)
    {
        var status = await moolre.Payments.GetStatusAsync(new GetPaymentStatusRequest
        {
            IdKind = TransactionIdKind.ExternalReference,
            Id = order.Id,
        }, cancellationToken);

        return status.Transaction.Status == TransactionStatus.Successful;
    }
}
```

## Several wallets

Moolre's credentials belong to one wallet each: a key for one wallet cannot act on another. Register each wallet by name and ask the factory for it:

```csharp
builder.Services.AddMoolre("collections", o => { o.ApiUser = "..."; o.PublicKey = "..."; o.AccountNumber = "..."; });
builder.Services.AddMoolre("payouts", o => { o.ApiUser = "..."; o.PrivateKey = "..."; o.AccountNumber = "..."; });

public class PayoutService(MoolreClientFactory moolre)
{
    public Task SendAsync(SendTransferRequest request, CancellationToken cancellationToken) =>
        moolre.Create("payouts").Transfers.SendAsync(request, cancellationToken);
}
```

See [Configuration](docs/configuration.md) for every setting.

## Exception handling

| Exception | Meaning | What to do |
| --- | --- | --- |
| `MoolreOutcomeUncertainException` | A call that moves money or sends a message timed out, lost its connection or got an unreadable answer. It **may or may not** have happened. | Check its status with the same `ExternalReference`. Never resend blindly. |
| `MoolreDuplicateReferenceException` | The external reference was already used. | Look up the original; do not resend with a new reference. |
| `MoolreAuthenticationException` | Credentials or wallet rejected, or API access not activated. | Fix the configuration. |
| `MoolreApiException` | Moolre refused the call (read `Code`, `Message`, `Detail`). For money calls, nothing moved. | Show the user or fix the request. |
| `MoolreResponseException` | Moolre's answer could not be read. | Try a read-only call again later. |

```csharp
try
{
    await moolre.Transfers.SendAsync(request, cancellationToken);
}
catch (MoolreOutcomeUncertainException ex)
{
    // ex.ExternalReference: look the transfer up with Transfers.GetStatusAsync before doing anything else.
}
catch (MoolreApiException ex) when (ex.Code == MoolreResponseCode.InsufficientWalletBalance)
{
    // Top up the wallet.
}
```

A bad request (a missing value, an invalid amount) throws an `ArgumentException` **before anything is sent**. Answers that look like failures but are normal results, such as "name not found" or "one-time code required", come back as results. See [Errors and money safety](docs/errors.md).

## Retries and timeouts

- **No automatic retries.** Every Moolre call is a `POST`, including payments and transfers, so a blind retry could charge a customer or send money twice. Do not add a retry policy to the SDK's `HttpClient`.
- **Read-only calls** (status lookups, lists, balances, the name check) are safe to repeat after a `TimeoutException` or `HttpRequestException`.
- **Timeout:** `MoolreOptions.Timeout`, 30 seconds by default. A money call that times out throws `MoolreOutcomeUncertainException` carrying your reference.

## Webhooks

```csharp
builder.Services.AddMoolreWebhooks(o => o.Secret = builder.Configuration["Moolre:CallbackSecret"]);

app.MapPost("/moolre/callback", async (HttpRequest http, MoolreWebhookVerifier verifier) =>
{
    using var body = new MemoryStream();
    await http.Body.CopyToAsync(body);
    var rawBody = body.ToArray();

    if (!verifier.IsSignatureValid(rawBody, http.Headers[MoolreWebhookVerifier.SignatureHeaderName])) return Results.Unauthorized();
    if (!PaymentWebhook.TryParse(rawBody, out var webhook)) return Results.BadRequest();

    if (webhook.IsSuccessfulPayment)
    {
        // Confirm with Payments.GetStatusAsync, then fulfil the order exactly once (deduplicate on webhook.EventId).
    }

    return Results.Ok();
});
```

The `X-Moolre-Signature` header (an HMAC-SHA256 of the raw body, keyed with the wallet's callback secret) is checked in constant time, and callbacks more than 5 minutes old are rejected. For several wallets, set one secret per wallet in `WalletSecrets`. See [Payment webhooks](docs/webhooks.md), including Moolre's callback addresses for your firewall.

## Documentation

- [User guides](docs/README.md): getting started, configuration, payments, transfers, accounts, sales, SMS, WhatsApp, webhooks, errors and money safety, testing your code
- [API reference](docs/reference.md): every method, with its signature and what it does
- [Architecture](docs/architecture.md): how the SDK works inside, with diagrams
- [Changelog](CHANGELOG.md) · [Troubleshooting](TROUBLESHOOTING.md) · [Security policy](SECURITY.md)

## Contributing

```bash
dotnet build -c Release
dotnet test -c Release
```

The .NET SDK is pinned in `global.json`; warnings are errors. Read [CONTRIBUTING.md](CONTRIBUTING.md) for where code goes, the tests each change needs, and the sandbox tests. Please follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## License

[MIT](LICENSE)
