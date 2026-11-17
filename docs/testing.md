# Testing your code

Every client in the SDK is an abstract class with overridable (`virtual`) methods, so your own tests can replace it with a test double, with any mocking library or by hand.

## With a mocking library

```csharp
// Example with NSubstitute; Moq and FakeItEasy work the same way.
var payments = Substitute.For<MoolrePayments>();
var client = Substitute.For<MoolreClient>();
client.Payments.Returns(payments);

var service = new CheckoutService(client);
```

## By hand

```csharp
sealed class FakeTransfers : MoolreTransfers
{
    public List<SendTransferRequest> Sent { get; } = [];

    public override Task<TransferResult> SendAsync(SendTransferRequest request, CancellationToken cancellationToken = default)
    {
        Sent.Add(request);
        throw new MoolreOutcomeUncertainException("timed out", "Transfers.SendAsync", request.ExternalReference);
    }
}
```

Throwing the SDK's own exceptions from a double is the easiest way to test how your code handles an unknown outcome, a duplicate reference or a refusal; all exception types have public constructors.

A member you do not override throws `NotImplementedException`, so a test never silently gets an empty answer.

## Building results

Result types (for example `TransferResult`) have internal constructors, because only the SDK reads Moolre's answers. A factory for building results in your own tests (`MoolreModelFactory`) is planned but not available yet. Until then, test your code's decisions through exceptions and the requests your double receives, as above.

## Against the sandbox

Use `MoolreEnvironment.Sandbox` and your sandbox credentials. The sandbox does not move real money, and `RequestMobileMoneyRequest.SkipOtp = true` skips the one-time code step there.

What the sandbox did not support when checked on 17 November 2026:

- **Sales** (airtime, data, bundles): `sandbox.moolre.com/open/sales/...` answers HTTP 404 with an HTML page; the live API serves these paths.
- **Bank collection accounts** (`CreateBankAccountAsync`): the sandbox answers "Request type invalid".
- **Update Account** without `AccountName`: the sandbox rejects it, although Moolre's docs mark the name optional. Send the name with every update.
