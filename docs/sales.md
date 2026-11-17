# Sales (airtime and data)

`moolre.Sales` sells airtime and data bundles from your wallet. Your wallet must be linked to Moolre's **Sell on Our Platform** service. It uses your **private key**.

## Airtime

```csharp
using Zhuwa.Moolre;
using Zhuwa.Moolre.Sales;

var sale = await moolre.Sales.SendAirtimeAsync(new SendAirtimeRequest
{
    Network = SalesNetwork.Mtn,              // Mtn, Telecel or At
    ReceiverPhoneNumber = "233240000000",
    Amount = 5m,                             // Moolre's minimum is GHS 1.00
    ExternalReference = "AIR-1001",          // unique per sale
}, cancellationToken);

if (sale.IsInProgress)
{
    // Moolre accepted it but has not delivered it yet (ASL11). Check with GetStatusAsync.
}
```

## Data bundles

Pick a bundle from the list, then sell it. The bundle's own price is charged; no amount is sent.

```csharp
var bundles = await moolre.Sales.ListBundlesAsync(cancellationToken);
var bundle = bundles.Mtn.First(b => b.Label == "406.89 MB");

var sale = await moolre.Sales.SendDataAsync(new SendDataRequest
{
    Network = SalesNetwork.Mtn,
    ReceiverPhoneNumber = "233240000000",
    BundleId = bundle.BundleId,
    ExternalReference = "DATA-1001",
}, cancellationToken);
```

Each `DataBundle` has `BundleId`, `Price`, `Label` (for example "406.89 MB"), `Megabytes`, `Validity` and `Category`.

## Sale status

```csharp
var status = await moolre.Sales.GetStatusAsync(new GetSaleStatusRequest
{
    IdKind = SaleIdKind.ExternalReference,   // or TransactionId, SaleReference
    Id = "AIR-1001",
}, cancellationToken);

switch (status.Status)
{
    case SaleStatus.Sent: break;               // delivered
    case SaleStatus.Pending: break;            // still processing
    case SaleStatus.FailedAndRefunded: break;  // your wallet was refunded
    case SaleStatus.NotFound: break;           // no sale with that ID
}
```

## When something goes wrong

| What happens | What you get |
| --- | --- |
| In progress (`ASL11`) | `SaleResult` with `IsInProgress = true` |
| Moolre's "unknown error, the sale may still have gone through" (`ASL06`) | `MoolreOutcomeUncertainException` with your reference: check the status |
| Reference already used (`ASL08`) | `MoolreDuplicateReferenceException` |
| Refused, for example amount under GHS 1.00 (`ASL01`) | `MoolreApiException`: nothing was spent |
| Timeout, dropped connection | `MoolreOutcomeUncertainException` with your reference |

Never retried by the SDK.
