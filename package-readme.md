# Zhuwa.Moolre.Sdk

An **unofficial, community-built** .NET client for the [Moolre](https://docs.moolre.com/) API, maintained by [Zhuwa Africa](https://github.com/zhuwaafrica). It covers all 31 documented Moolre operations: mobile money collections, payment links, Payment IDs and bank collections, transfers, wallets, airtime and data, SMS and WhatsApp, plus payment webhooks.

> This project is independent. It is not affiliated with, endorsed by, or supported by Moolre.

## Requirements

- .NET 10 or later. Works with trimming and Native AOT.
- Moolre API credentials for your wallet (created in the Moolre app; each wallet has its own).

## Installation

```bash
dotnet add package Zhuwa.Moolre.Sdk --prerelease
```

## Usage

Register the SDK once, then inject `MoolreClient`:

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.DependencyInjection;
using Zhuwa.Moolre.Payments;

builder.Services.AddMoolre(options =>
{
    options.Environment = MoolreEnvironment.Sandbox;   // the default; Live is an explicit choice
    options.ApiUser = builder.Configuration["Moolre:ApiUser"];
    options.PublicKey = builder.Configuration["Moolre:PublicKey"];
    options.AccountNumber = builder.Configuration["Moolre:AccountNumber"];
});

public class CheckoutService(MoolreClient moolre)
{
    public async Task StartAsync(string orderId, CancellationToken cancellationToken)
    {
        var result = await moolre.Payments.RequestMobileMoneyAsync(new RequestMobileMoneyRequest
        {
            Channel = PaymentChannel.Mtn,
            Currency = Currency.Ghs,
            PayerPhoneNumber = "0241234567",
            Amount = 25.50m,
            ExternalReference = orderId,   // unique per payment; keep it
        }, cancellationToken);

        // result.Outcome: PromptSent, or OtpRequired (send again with the customer's code)
    }
}
```

Several wallets: register each with `AddMoolre("name", ...)` and use `MoolreClientFactory.Create("name")`.

## Exception handling

| Exception | Meaning |
| --- | --- |
| `MoolreOutcomeUncertainException` | A money or message call may or may not have happened (timeout, dropped connection). Check its status with the same reference; never resend blindly. |
| `MoolreDuplicateReferenceException` | The reference was already used. Look up the original. |
| `MoolreAuthenticationException` | Credentials or wallet rejected. |
| `MoolreApiException` | Moolre refused the call; for money calls, nothing moved. |
| `MoolreResponseException` | Moolre's answer could not be read. |

Calls that move money or send messages are **never retried** by the SDK. Every Moolre call is a `POST`, so do not add a blind retry policy to its `HttpClient`.

## Webhooks

```csharp
builder.Services.AddMoolreWebhooks(o => o.Secret = builder.Configuration["Moolre:CallbackSecret"]);

// In your endpoint, with the raw body bytes and the X-Moolre-Signature header:
if (verifier.IsSignatureValid(body, signature) && PaymentWebhook.TryParse(body, out var webhook) && webhook.IsSuccessfulPayment)
{
    // confirm with Payments.GetStatusAsync, then fulfil once
}
```

## Documentation

- [User guides](https://github.com/zhuwaafrica/moolre-dotnet-sdk/tree/main/docs): getting started, configuration, every API area, webhooks, errors and money safety, testing
- [Changelog](https://github.com/zhuwaafrica/moolre-dotnet-sdk/blob/main/CHANGELOG.md)
- [Source code](https://github.com/zhuwaafrica/moolre-dotnet-sdk) and [issues](https://github.com/zhuwaafrica/moolre-dotnet-sdk/issues)
- [MIT licence](https://github.com/zhuwaafrica/moolre-dotnet-sdk/blob/main/LICENSE)
