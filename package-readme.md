# Zhuwa.Moolre.Sdk

An **unofficial, community-built** .NET client for the [Moolre](https://docs.moolre.com/) API, maintained by [Zhuwa Africa](https://github.com/zhuwaafrica).

> This project is independent. It is not affiliated with, endorsed by, or supported by Moolre.

## Status: preview, calls not implemented yet

The request and response types exist, but **no call to Moolre is implemented yet**. Do not use it in production. The usage below is the design, not what ships today.

## What it will cover

One package for the whole documented Moolre API: mobile money collections, payment links, Payment IDs and bank collections, transfers and name validation, wallet status and transactions, airtime and data sales, SMS and sender IDs, WhatsApp templates and messages, and payment webhook models.

## Planned usage (dependency injection first)

```csharp
builder.Services.AddMoolre(options =>
{
    options.Environment = MoolreEnvironment.Sandbox;   // sandbox is the default; live is an explicit choice
    options.ApiUser = "...";                           // from configuration or a secret store, never source code
    options.PublicKey = "...";
    options.PrivateKey = "...";
    options.AccountNumber = "...";
});

public class CheckoutService(MoolreClient moolre)
{
    // await moolre.Payments.RequestMobileMoneyAsync(...);
    // await moolre.Payments.GetStatusAsync(...);
}
```

## Money safety

- Payment, transfer, sales and messaging calls are **never retried automatically**.
- You supply the external reference. After a timeout the SDK throws an exception that carries it, so you can check the status instead of paying twice.
- A callback or a browser redirect is not proof of payment. Confirm the status before fulfilling an order.
- If you add a resilience handler to the `HttpClient`, do not retry `POST` requests blindly: every Moolre call is a `POST`, including read-only status lookups.

## Requirements

- .NET 10 or later.

## Links

- [Source code and documentation](https://github.com/zhuwaafrica/moolre-dotnet-sdk)
- [Changelog](https://github.com/zhuwaafrica/moolre-dotnet-sdk/blob/main/CHANGELOG.md)
- [Report a problem](https://github.com/zhuwaafrica/moolre-dotnet-sdk/issues)
- [MIT licence](https://github.com/zhuwaafrica/moolre-dotnet-sdk/blob/main/LICENSE)
