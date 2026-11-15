# Moolre .NET SDK

An **unofficial, community-built** .NET client for the [Moolre](https://docs.moolre.com/) API, maintained by [Zhuwa Africa](https://github.com/zhuwaafrica). NuGet package: `Zhuwa.Moolre.Sdk`.

> [!NOTE]
> This project is independent. It is not affiliated with, endorsed by, or supported by Moolre.

> [!WARNING]
> **Status: preview.** The request, result and model types and the endpoint table exist and are tested, but **only two Moolre calls work so far** (`Accounts.GetStatusAsync` and `ReferenceData.GetBanksAsync`); the other 29 are not implemented yet. The diagrams below show how the SDK is designed to work. The NuGet page text is in [`package-readme.md`](package-readme.md).

**On this page:** [How it fits together](#how-it-fits-together) · [Collecting a payment](#collecting-a-payment) · [Receiving a webhook](#receiving-a-webhook) · [When something goes wrong](#when-something-goes-wrong) · [Repository layout](#repository-layout) · [Build and test](#build-and-test)

## Planned usage

Dependency injection first: register once, then inject `MoolreClient` wherever you need it.

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

## How it fits together

Your code only ever touches the public part of the SDK. Everything below the area clients is internal, so it can change without breaking you.

```mermaid
flowchart LR
    app["<b>Your application</b><br/>ASP.NET Core, worker, console"]

    subgraph sdk["Zhuwa.Moolre.Sdk"]
        di["<b>AddMoolre(...)</b><br/>options, validation at startup,<br/>HttpClient setup"]
        client["<b>MoolreClient</b><br/>the one object you inject"]

        subgraph areas["API areas (public)"]
            acc["Accounts"]
            tr["Transfers"]
            pay["Payments"]
            sales["Sales"]
            sms["Sms"]
            wa["WhatsApp"]
            ref["ReferenceData"]
        end

        subgraph internal["Internal (never public)"]
            transport["<b>Transport</b><br/>picks the right credentials<br/>for each call and sends it"]
            json["<b>Serialization</b><br/>tolerant JSON converters<br/>and response envelope"]
            guard["<b>Validation and logging</b><br/>no secrets in logs"]
        end

        di --> client
        client --> areas
        areas --> transport
        transport --> json
        transport --> guard
    end

    factory["IHttpClientFactory<br/>pooled HttpClient"]
    moolre[("<b>Moolre API</b><br/>sandbox or live")]

    app -->|"inject"| client
    transport --> factory
    factory --> moolre
```

Which credentials each area uses (Moolre issues separate keys per wallet and per service):

```mermaid
flowchart LR
    subgraph priv["Private key: X-API-USER + X-API-KEY"]
        a1["Accounts"]
        a2["Transfers: send, internal"]
        a3["Sales"]
    end
    subgraph pub["Public key: X-API-USER + X-API-PUBKEY"]
        b1["Payments: collections,<br/>links, Payment IDs, bank accounts"]
    end
    subgraph either["Either key"]
        c1["Transfers: validate name,<br/>transfer status"]
    end
    subgraph vas["VAS key: X-API-VASKEY (separate for each)"]
        d1["Sms"]
        d2["WhatsApp"]
    end
    subgraph none["No credentials"]
        e1["ReferenceData: bank list"]
    end
```

## Collecting a payment

A mobile money collection is asynchronous: the SDK asks Moolre to prompt the customer, and the final result arrives later by webhook. Always confirm with a status lookup before fulfilling an order.

```mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant App as Your app
    participant SDK as MoolreClient.Payments
    participant API as Moolre API
    participant Hook as Your webhook endpoint

    App->>App: Save the order with a unique externalref
    App->>SDK: RequestMobileMoneyAsync(request)
    SDK->>API: POST /open/transact/payment
    opt Moolre asks for a one-time code (TP14, status 1)
        API-->>SDK: OTP required
        SDK-->>App: MobileMoneyResult (OtpRequired)
        App->>SDK: RequestMobileMoneyAsync(request with OtpCode)
        SDK->>API: POST /open/transact/payment
    end
    API-->>SDK: Prompt triggered (TR099)
    SDK-->>App: MobileMoneyResult (PromptSent)
    API->>Customer: Prompt on the customer's phone
    Customer->>API: Approves the payment
    API->>Hook: POST callback (P01 success or P02 failed)
    Hook->>SDK: Payments.GetStatusAsync(externalref)
    SDK->>API: POST /open/transact/status
    API-->>SDK: Transaction (SS01)
    SDK-->>Hook: TransactionResult
    Hook->>App: Fulfil the order exactly once
```

## Receiving a webhook

Moolre calls your endpoint when a payment succeeds or fails. The SDK gives you a parser and a secret check; deciding what to do stays in your code.

```mermaid
flowchart LR
    moolre["Moolre"] -->|"POST callback from<br/>6 documented IP addresses"| endpoint["Your webhook endpoint"]
    endpoint --> parse["PaymentWebhook.TryParse"]
    parse --> verify{"Secret matches and<br/>externalref is yours?"}
    verify -- no --> reject["Reject and log<br/>(never log the secret)"]
    verify -- yes --> seen{"Already processed?"}
    seen -- yes --> ack["Acknowledge, do nothing"]
    seen -- no --> ok{"P01 and txstatus 1?"}
    ok -- no --> fail["Record the failure and its reason<br/>never fulfil from P02"]
    ok -- yes --> confirm["Confirm with GetStatusAsync<br/>(especially high-value orders)"]
    confirm --> fulfil["Mark paid and fulfil once"]
```

## When something goes wrong

Moolre can answer with an error, or not answer at all. The SDK treats the two very differently, because a timeout on a payment does not mean the payment failed.

```mermaid
flowchart TD
    send["Send the request"] --> got{"Response received?"}

    got -- "no: timeout or network error" --> money{"Does it move money<br/>or send a message?"}
    money -- yes --> uncertain["Throw MoolreOutcomeUncertainException<br/>carrying your externalref<br/>(never retried automatically)"]
    money -- "no: read-only call" --> neterr["Network error reaches your code"]

    got -- yes --> parse{"Body understood?"}
    parse -- no --> bad["Throw MoolreResponseException"]
    parse -- yes --> code{"Which application code?"}

    code -- "AIN01, AIN03, AIN04, IN01,<br/>ASL00, ASS00" --> auth["Throw MoolreAuthenticationException"]
    code -- "TP13, TR013, APY18,<br/>ASL08, INP02" --> dup["Throw MoolreDuplicateReferenceException"]
    code -- "other business failure<br/>(status 0)" --> api["Throw MoolreApiException"]
    code -- "success, or a valid outcome:<br/>pending, OTP required, not found" --> result["Return a typed result<br/>(not an exception)"]
```

What to do after an uncertain outcome:

```mermaid
sequenceDiagram
    participant App as Your app
    participant SDK as MoolreClient.Transfers
    participant API as Moolre API

    App->>SDK: SendAsync(externalref = T-1001)
    SDK->>API: POST /open/transact/transfer
    Note over SDK,API: Timeout. The transfer may or may not have happened.
    SDK--xApp: MoolreOutcomeUncertainException (T-1001)
    Note over App: Do not resend. Do not make a new reference.
    App->>SDK: GetStatusAsync(externalref = T-1001)
    SDK->>API: POST /open/transact/status
    API-->>SDK: Transaction found, or not found
    SDK-->>App: TransactionResult
    Note over App: Decide only once the status is final.
```

## Money safety

- Payment, transfer, sales and messaging calls are **never retried automatically**.
- You supply the external reference. After a timeout the SDK throws an exception that carries it, so you can check the status instead of paying twice.
- A callback or a browser redirect is not proof of payment. Confirm the status before fulfilling an order.
- If you add a resilience handler to the `HttpClient`, do not retry `POST` requests blindly: every Moolre call is a `POST`, including read-only status lookups.

## Repository layout

| Path | Contents |
| --- | --- |
| `src/Zhuwa.Moolre.Sdk/` | The library (`net10.0`). One folder per API area (`Accounts`, `Transfers`, `Payments`, `Sales`, `Sms`, `WhatsApp`, `Reference`). Each area keeps everything for that feature together: the public client, its internal implementation (`DefaultMoolre<Area>`) and response mapper (`<Area>Mapper`) at the folder root, plus `Enums/` (C# enums only), `Models/` (data types and text-based value types), `Requests/` and `Results/` as needed. Shared core types use the same folder names at the project root, plus `Clients/` and `Configuration/`; the response codes are in `Models/ResponseCodes/`. Also `Errors/`, `Webhooks/`, `DependencyInjection/`, and `Internal/` (shared plumbing: transport, serialization, options validation; never public API) |
| `src/Zhuwa.Moolre.Sdk/.PublicAPI/` | The reviewed public API surface (`Shipped` and `Unshipped`); an unlisted public change fails the build |
| `tests/Zhuwa.Moolre.Sdk.Tests/` | xunit v3 tests mirroring the source layout; `Fixtures/` holds Moolre's documented example responses; no network, no credentials |
| `samples/` | Sample applications (added with the code they exercise) |
| `Directory.Build.props`, `Directory.Packages.props`, `BannedSymbols.txt`, `.editorconfig`, `global.json` | Shared build settings, central package versions, banned APIs, analyzer rules, pinned SDK |

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack src/Zhuwa.Moolre.Sdk -c Release
```

The .NET SDK is pinned in `global.json`. Build output goes to `artifacts/`. Warnings are errors; see [CONTRIBUTING.md](CONTRIBUTING.md).

## More

- [Troubleshooting](TROUBLESHOOTING.md)
- [Changelog](CHANGELOG.md)
- [Contributing](CONTRIBUTING.md), [Security policy](SECURITY.md) and [Code of Conduct](CODE_OF_CONDUCT.md)

## License

[MIT](LICENSE)
