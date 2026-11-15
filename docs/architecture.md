# Architecture

How the SDK works inside, for contributors and anyone curious. Using the SDK does not require any of this; start with [Getting started](getting-started.md).

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

Moolre documents no signature, so `MoolreWebhookVerifier.IsSecretValid` (the callback secret, compared in constant time) is the verification. Moolre also publishes the addresses it sends callbacks from. Filter them in your firewall, load balancer or reverse proxy, not in application code; Moolre can change them, so check its documentation:

| IPv4 | IPv6 |
| --- | --- |
| `174.138.44.22` | `2604:a880:400:d1:0:3:4cf0:c001` |
| `174.138.40.67` | `2604:a880:400:d0::1a77:4001` |
| `192.241.135.134` | `2604:a880:400:d0::4234:2001` |

(As listed on Moolre's payment webhook page, read 14 November 2026.)

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

## Repository layout

| Path | Contents |
| --- | --- |
| `src/Zhuwa.Moolre.Sdk/` | The library (`net10.0`), laid out exactly like the official Square .NET SDK. Root: `MoolreClient` and `MoolreClientFactory` only. One folder per API area (`Accounts`, `Transfers`, `Payments`, `Sales`, `Sms`, `WhatsApp`, `Reference`, `Webhooks`) with the area client, its internal implementation and mapper, and a `Requests/` folder. `Types/`: every result, model, enum and response code, grouped into one subfolder per area plus `_common/` for shared types (Stripe groups its `Entities/` the same way). `Core/`: internal plumbing (transport, serialization, validation); `Core/Public/`: options, environment and exceptions. `DependencyInjection/`: `AddMoolre` |
| `src/Zhuwa.Moolre.Sdk/.PublicAPI/` | The reviewed public API surface (`Shipped` and `Unshipped`); an unlisted public change fails the build |
| `tests/Zhuwa.Moolre.Sdk.Tests/` | xunit v3 unit tests mirroring the source layout; `Fixtures/` holds Moolre's documented example responses; no network, no credentials |
| `tests/Zhuwa.Moolre.Sdk.IntegrationTests/` | Tests against Moolre's real sandbox, skipped unless you opt in with environment variables (see its README) |
| `samples/` | Sample applications (added with the code they exercise) |
| `Directory.Build.props`, `Directory.Packages.props`, `BannedSymbols.txt`, `.editorconfig`, `global.json` | Shared build settings, central package versions, banned APIs, analyzer rules, pinned SDK |
