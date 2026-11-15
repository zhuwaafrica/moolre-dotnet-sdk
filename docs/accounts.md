# Accounts

`moolre.Accounts` reads and changes your wallet. It uses your **private key**.

## Balance and settings

```csharp
var wallet = await moolre.Accounts.GetStatusAsync(cancellationToken);   // the wallet in MoolreOptions.AccountNumber

Console.WriteLine($"{wallet.AccountName}: {wallet.Balance} (activated: {wallet.IsActivated})");
```

Pass a `GetAccountStatusRequest { AccountNumber = "..." }` to read another wallet your credentials belong to.

## Change wallet settings

Only the values you set are sent, so nothing else on the wallet changes:

```csharp
var updated = await moolre.Accounts.UpdateAsync(new UpdateAccountRequest
{
    CallbackUrl = new Uri("https://shop.example.com/moolre/callback"),
}, cancellationToken);
```

Set `ApiEnabled` only when you want to change API access. The result includes the wallet's callback secret (`updated.Account.Secret`): store it securely and use it for [webhooks](webhooks.md); never log it.

## Transaction history

```csharp
var history = await moolre.Accounts.ListTransactionsAsync(new ListAccountTransactionsRequest
{
    From = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
    To = DateTimeOffset.UtcNow,
    Status = TransactionStatus.Successful,   // optional filter
    Limit = 100,                              // optional
}, cancellationToken);

foreach (var transaction in history.Transactions) { /* ... */ }
```

Dates are sent in GMT, whatever your server's time zone. "No transactions" is an empty list, not an error.

## Sandbox wallets

In the sandbox you can create test wallets from code (Moolre does not offer this in live; create live wallets in the Moolre app):

```csharp
var created = await moolre.Accounts.CreateSandboxAccountAsync(new CreateSandboxAccountRequest
{
    AccountName = "My Test Shop",
    CallbackUrl = new Uri("https://example.com/moolre/callback"),
    Settlement = new SettlementSettings(),
}, cancellationToken);

Console.WriteLine(created.Account.AccountNumber);
```

The SDK refuses this call when it is set to live.
