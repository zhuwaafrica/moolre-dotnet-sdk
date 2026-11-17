# Configuration

## Settings (`MoolreOptions`)

| Setting | Default | What it does |
| --- | --- | --- |
| `Environment` | `Sandbox` | `Sandbox` or `Live`. Live is always an explicit choice. |
| `ApiUser` | none | Your API user (`X-API-USER`). |
| `PrivateKey` | none | Private key (`X-API-KEY`). Required in live for transfers, sales and accounts. |
| `PublicKey` | none | Public key (`X-API-PUBKEY`). Required in live for payments. |
| `SmsVasKey` | none | SMS service key (`X-API-VASKEY`). |
| `WhatsAppVasKey` | none | WhatsApp service key (`X-API-VASKEY`, a different key from SMS). |
| `AccountNumber` | none | The wallet calls act on when a request does not name one. |
| `Timeout` | 30 seconds | How long a call may take. Must be greater than zero. |
| `BaseAddress` | none | For tests only: an `https` address to send to instead of Moolre. |

Each call sends **only** the credentials it needs: an SMS call never carries your wallet keys, and reference data carries none.

## Reading settings from `appsettings.json`

```json
{
  "Moolre": {
    "Environment": "Sandbox",
    "AccountNumber": "10772306069982"
  }
}
```

Keep keys out of `appsettings.json`; use user secrets in development (`dotnet user-secrets set "Moolre:ApiUser" "..."`) and environment variables or a secret store in production. Then copy the values in `AddMoolre`:

```csharp
builder.Services.AddMoolre(options =>
{
    var section = builder.Configuration.GetSection("Moolre");
    options.Environment = Enum.Parse<MoolreEnvironment>(section["Environment"] ?? "Sandbox");
    options.ApiUser = section["ApiUser"];
    options.PrivateKey = section["PrivateKey"];
    options.PublicKey = section["PublicKey"];
    options.SmsVasKey = section["SmsVasKey"];
    options.WhatsAppVasKey = section["WhatsAppVasKey"];
    options.AccountNumber = section["AccountNumber"];
});
```

Copying values explicitly (instead of reflection-based binding) keeps your app safe for trimming and Native AOT.

## What is checked at startup

- `Timeout` is greater than zero.
- `BaseAddress`, if set, is an absolute `https` address.
- `Environment` is `Sandbox` or `Live`.
- No credential or account number contains a line break or other control character.

A failing check stops the app at startup with a message naming the setting.

## Several wallets

Moolre issues credentials per wallet: a key for one wallet cannot act on another. Register each wallet by name:

```csharp
builder.Services.AddMoolre("collections", o =>
{
    o.ApiUser = builder.Configuration["Moolre:Collections:ApiUser"];
    o.PublicKey = builder.Configuration["Moolre:Collections:PublicKey"];
    o.AccountNumber = builder.Configuration["Moolre:Collections:AccountNumber"];
});

builder.Services.AddMoolre("payouts", o =>
{
    o.Environment = MoolreEnvironment.Live;
    o.ApiUser = builder.Configuration["Moolre:Payouts:ApiUser"];
    o.PrivateKey = builder.Configuration["Moolre:Payouts:PrivateKey"];
    o.AccountNumber = builder.Configuration["Moolre:Payouts:AccountNumber"];
});
```

Then inject `MoolreClientFactory` and ask for the wallet you need:

```csharp
public class PayoutService(MoolreClientFactory moolre)
{
    public Task PayAsync(SendTransferRequest request, CancellationToken cancellationToken) =>
        moolre.Create("payouts").Transfers.SendAsync(request, cancellationToken);
}
```

- Each wallet has its own settings, checked at startup, and its own environment.
- The same name always returns the same client; it is safe to keep and share.
- An unknown name throws `InvalidOperationException` naming the wallet, so a typo never uses another wallet's keys.
- You can also call `AddMoolre(options => ...)` without a name; that wallet is the `MoolreClient` you inject directly.

## HTTP

The SDK sends through `IHttpClientFactory`, with one named client per wallet: `Zhuwa.Moolre` for the unnamed wallet and `Zhuwa.Moolre:<name>` for named ones. You can add your own handlers (logging, a proxy) to them:

```csharp
builder.Services.AddHttpClient("Zhuwa.Moolre").AddHttpMessageHandler<MyLoggingHandler>();
```

Do **not** add an automatic retry policy: every Moolre call is a `POST`, including payments and transfers, and a blind retry can charge a customer or send money twice. See [Errors and money safety](errors.md).

The SDK does not follow redirects (credentials are custom headers, which .NET would forward to another host) and refuses response bodies over 16 MB.
