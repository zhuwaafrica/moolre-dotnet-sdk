# Transfers

`moolre.Transfers` sends money from your wallet. Sending uses your **private key**; the name check and status accept either key.

## Check the name first

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Transfers;

var name = await moolre.Transfers.ValidateNameAsync(new ValidateNameRequest
{
    Receiver = "0246798993",
    Channel = TransferChannel.Mtn,      // Mtn, Telecel, At or Bank
    Currency = Currency.Ghs,
}, cancellationToken);

if (!name.Found)
{
    // Unknown number: a normal result, not an exception.
}
else
{
    Console.WriteLine(name.AccountHolderName);   // show it to your user before sending
}
```

For a bank account, set `Channel = TransferChannel.Bank` and `BankCode` (from `moolre.ReferenceData.GetBanksAsync("gha")`).

## Send a transfer

```csharp
var transfer = await moolre.Transfers.SendAsync(new SendTransferRequest
{
    Channel = TransferChannel.Mtn,
    Currency = Currency.Ghs,
    Amount = 50m,
    Receiver = "0246798993",
    ExternalReference = payout.Id,      // unique per transfer; save it first
    Narration = "Salary",               // optional
}, cancellationToken);

if (transfer.Status == TransactionStatus.Pending)
{
    // In progress (Moolre's OTGH02, or no status sent). Confirm later with GetStatusAsync.
}
```

| What happens | What you get |
| --- | --- |
| Paid out (`OBGH01`) | `TransferResult`, `Status = Successful` |
| In progress (`OTGH02`) | `TransferResult`, `Status = Pending` |
| Reference already used (`APY18`) | `MoolreDuplicateReferenceException` |
| Refused, for example insufficient balance (`APY21`) or payout failed (`APY17`) | `MoolreApiException`: no money moved |
| Timeout, dropped connection, unreadable answer | `MoolreOutcomeUncertainException` with your reference |

The bank code is sent only for bank transfers. Never retried by the SDK.

## Transfer status

```csharp
var status = await moolre.Transfers.GetStatusAsync(new GetTransferStatusRequest
{
    IdKind = TransactionIdKind.ExternalReference,
    Id = payout.Id,
}, cancellationToken);
```

## Wallet to wallet

```csharp
var internalTransfer = await moolre.Transfers.SendInternalAsync(new SendInternalTransferRequest
{
    Currency = Currency.Ghs,
    Amount = 25m,
    ReceiverAccountNumber = "10000010002",   // a Moolre wallet number, not a phone
    ExternalReference = "I-1001",
}, cancellationToken);

// Moolre only confirms that it started (internalTransfer.TransferId). Check the final state with GetStatusAsync.
```
