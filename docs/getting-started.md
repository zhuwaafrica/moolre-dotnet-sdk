# Getting started

## 1. Install

```bash
dotnet add package Zhuwa.Moolre.Sdk
```

The package targets .NET 10 and works with trimming and Native AOT.

## 2. Get your credentials from Moolre

Moolre gives each **wallet** its own API credentials, created in the Moolre app (an "API Service" per wallet). Depending on what you call, you need:

| Credential | Setting | Used by |
| --- | --- | --- |
| API user | `ApiUser` | Accounts, Transfers, Payments, Sales |
| Private key | `PrivateKey` | Transfers (sending), Sales, Accounts |
| Public key | `PublicKey` | Payments (collections, links, Payment IDs, bank accounts) |
| SMS VAS key | `SmsVasKey` | SMS |
| WhatsApp VAS key | `WhatsAppVasKey` | WhatsApp (a different key from SMS) |
| Wallet account number | `AccountNumber` | The wallet calls act on, unless a request names another |

The sandbox does not need the private and public keys, so you can start with just the API user. Keys generated in the Moolre app are live keys: they do not work in the sandbox (Moolre: "live keys never work in the sandbox"). SMS and WhatsApp need their keys in the sandbox too.

## 3. Register the SDK

In `Program.cs`:

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.DependencyInjection;

builder.Services.AddMoolre(options =>
{
    options.Environment = MoolreEnvironment.Sandbox;   // the default; set Live when you go live
    options.ApiUser = builder.Configuration["Moolre:ApiUser"];
    options.PublicKey = builder.Configuration["Moolre:PublicKey"];
    options.PrivateKey = builder.Configuration["Moolre:PrivateKey"];
    options.AccountNumber = builder.Configuration["Moolre:AccountNumber"];
});
```

Keep the values in configuration, user secrets or a secret store, never in source code. The settings are checked when the app starts, so a mistake stops the app straight away instead of failing on the first payment.

## 4. Make a call

Inject `MoolreClient` (or just the part you need, such as `MoolrePayments`):

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Payments;

public class CheckoutService(MoolreClient moolre)
{
    public async Task<string> StartPaymentAsync(string orderId, string phone, decimal amount, CancellationToken cancellationToken)
    {
        var result = await moolre.Payments.RequestMobileMoneyAsync(new RequestMobileMoneyRequest
        {
            Channel = PaymentChannel.Mtn,
            Currency = Currency.Ghs,
            PayerPhoneNumber = phone,          // local format, for example 0241234567
            Amount = amount,
            ExternalReference = orderId,       // unique per payment; keep it
        }, cancellationToken);

        return result.Outcome.ToString();      // PromptSent, or OtpRequired (see the Payments guide)
    }
}
```

## 5. Go live

Set `options.Environment = MoolreEnvironment.Live` and use your live credentials. In live, the private and public keys are required for the calls that use them; a missing key fails before anything is sent.

## Next

- [Configuration](configuration.md) for every setting and several wallets.
- [Errors and money safety](errors.md) before you handle real money.
- [Payment webhooks](webhooks.md) to confirm payments.
