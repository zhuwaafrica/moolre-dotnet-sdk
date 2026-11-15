# API reference

Every method in the SDK, with its signature, what it does, its parameters and the exceptions it can throw. This page is generated from the SDK's own documentation comments, so it always matches the code; see the [user guides](README.md) for worked examples.

**Areas:** [Payments](#payments) · [Transfers](#transfers) · [Accounts](#accounts) · [Sales](#sales) · [SMS](#sms) · [WhatsApp](#whatsapp) · [Reference data](#reference-data) · [Setup](#setup) · [Webhooks](#webhooks)

## Payments

`moolre.Payments`. Uses the public key.

### `CreateBankAccountAsync`

```csharp
Task<BankAccountResult> moolre.Payments.CreateBankAccountAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
```

Creates a bank account number that customers pay into. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The account holder. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>CreateBankAccountRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `Amount` | `decimal?` |  | Amount. Moolre field `amount` (a JSON NUMBER on this endpoint only). |
| `FirstName` | `string` | yes | First name. Moolre field `firstname`. |
| `LastName` | `string` | yes | Last name. Moolre field `lastname`. |
| `PhoneNumber` | `string` | yes | Phone number. Moolre field `phone`. |
| `Email` | `string` | yes | Email. Moolre field `email`. |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The new account number, name and bank.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreOutcomeUncertainException` | The account may or may not have been created. |
| `MoolreApiException` | Moolre refused the request (for example `AD32`, the account could not be created). |

### `CreatePaymentIdAsync`

```csharp
Task<PaymentIdResult> moolre.Payments.CreatePaymentIdAsync(CreatePaymentIdRequest request, CancellationToken cancellationToken = default)
```

Creates a Payment ID that a customer dials to pay you. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The Payment ID. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>CreatePaymentIdRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `PhoneNumber` | `string` | yes | Phone number. Moolre field `phone` (+233 format, e.g. +233267606822). |
| `Name` | `string` | yes | Name. Moolre field `name`. |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `Amount` | `decimal?` |  | Amount. Moolre field `amount` (optional preset, greater than zero when sent). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The Payment ID.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid (for example a phone number not in +233 format). |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreOutcomeUncertainException` | The Payment ID may or may not have been created. |
| `MoolreApiException` | Moolre refused the request. |

### `CreatePaymentLinkAsync`

```csharp
Task<PaymentLinkResult> moolre.Payments.CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default)
```

Creates a hosted checkout link to send to the customer. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The link. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>CreatePaymentLinkRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Amount` | `decimal` | yes | Amount. Moolre field `amount` (string). |
| `Email` | `string` | yes | Email. Moolre field `email` (the business's email). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `CallbackUrl` | `Uri?` |  | Callback URL. Moolre field `callback`. |
| `RedirectUrl` | `Uri?` |  | Redirect URL. Moolre field `redirect`. |
| `IsReusable` | `bool` |  | Is reusable. Moolre field `reusable` ("0" / "1"). |
| `ExpiresAfter` | `TimeSpan?` |  | Expires after. Moolre field `expiration_time` (whole minutes, minimum 1). |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |
| `Metadata` | `IDictionary<string, string>` |  | Extra values Moolre stores with the payment and returns in the callback, for example an order ID. Moolre field `metadata`. Moolre documents it only as an object, so the SDK sends text values. |

</details>

**Returns:** The link and Moolre's reference for it.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid (for example an expiry under one minute). |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. |
| `MoolreOutcomeUncertainException` | The link may or may not have been created. |
| `MoolreResponseException` | Moolre reported success but sent no usable link. |
| `MoolreApiException` | Moolre refused the request. |

### `GetPaymentAccountAsync`

```csharp
Task<PaymentAccountLookupResult> moolre.Payments.GetPaymentAccountAsync(GetPaymentAccountRequest request, CancellationToken cancellationToken = default)
```

Looks up a Payment ID or bank account by the reference used to create it, with its payments. Safe to repeat. Not found is a normal result (`Found` is `false`), not an exception.

| Parameter | Description |
| --- | --- |
| `request` | Which Payment ID or bank account. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetPaymentAccountRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (used when creating the Payment ID or bank account). |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The account summary and its transactions, or a not-found result.

| Throws | When |
| --- | --- |
| `ArgumentException` | The external reference is missing. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `GetStatusAsync`

```csharp
Task<TransactionResult> moolre.Payments.GetStatusAsync(GetPaymentStatusRequest request, CancellationToken cancellationToken = default)
```

Gets the current state of a payment, by your external reference or by Moolre's ID. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | Which payment. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetPaymentStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `IdKind` | `TransactionIdKind` | yes | Which identifier `Id` is. Moolre field `idtype`. |
| `Id` | `string` | yes | Your external reference or Moolre's transaction ID. Moolre field `id`. |
| `AccountNumber` | `string?` |  | The wallet; the default from `AccountNumber` when omitted. Moolre field `accountnumber`. |

</details>

**Returns:** The transaction. Only `Successful` means the money arrived.

| Throws | When |
| --- | --- |
| `ArgumentException` | The ID is missing or the ID kind is not set. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `ListBankAccountsAsync`

```csharp
Task<BankAccountListResult> moolre.Payments.ListBankAccountsAsync()
```

Lists the bank account numbers created for the default wallet. Safe to repeat.

**Returns:** The accounts.

### `ListBankAccountsAsync`

```csharp
Task<BankAccountListResult> moolre.Payments.ListBankAccountsAsync(CancellationToken cancellationToken)
```

Lists the bank account numbers created for the default wallet. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The accounts.

### `ListBankAccountsAsync`

```csharp
Task<BankAccountListResult> moolre.Payments.ListBankAccountsAsync(ListBankAccountsRequest request, CancellationToken cancellationToken = default)
```

Lists the bank account numbers created for a wallet. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | Which wallet; the default from `AccountNumber` when its account number is omitted. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>ListBankAccountsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The accounts.

| Throws | When |
| --- | --- |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `RequestMobileMoneyAsync`

```csharp
Task<MobileMoneyResult> moolre.Payments.RequestMobileMoneyAsync(RequestMobileMoneyRequest request, CancellationToken cancellationToken = default)
```

Asks the customer to approve a mobile money payment on their phone. **Takes money**: never retried. Use a new, unique `ExternalReference` for each payment, and keep it.

| Parameter | Description |
| --- | --- |
| `request` | The payment. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`, because the prompt may have been sent. |

<details><summary><code>RequestMobileMoneyRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Channel` | `PaymentChannel` | yes | Channel. Moolre field `channel`. |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `PayerPhoneNumber` | `string` | yes | Payer phone number. Moolre field `payer` (local 0-prefixed format). |
| `Amount` | `decimal` | yes | Amount. Moolre field `amount` (string). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `OtpCode` | `string?` |  | OTP code. Moolre field `otpcode` (answer to an OTP challenge). |
| `Reference` | `string?` |  | Reference. Moolre field `reference`. |
| `SessionId` | `string?` |  | Session ID. Moolre field `sessionid` (USSD session). |
| `SkipOtp` | `bool?` |  | Skip OTP. Moolre field `skipotp` (sandbox only). |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** What happened: `PromptSent` (wait for the webhook or check the status) or `OtpRequired` (send the same request again with the customer's code in `OtpCode`).

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid, or `SkipOtp` is set outside the sandbox. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the public key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. Do not resend; check the original. |
| `MoolreOutcomeUncertainException` | The prompt may or may not have been sent. Check the status with the same reference. |
| `MoolreApiException` | Moolre refused the request (for example an unsupported currency). Nothing was charged. |

## Transfers

`moolre.Transfers`. Sending uses the private key; the name check and status accept either key.

### `GetStatusAsync`

```csharp
Task<TransactionResult> moolre.Transfers.GetStatusAsync(GetTransferStatusRequest request, CancellationToken cancellationToken = default)
```

Gets the current state of a transfer, by your external reference or by Moolre's ID. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | Which transfer. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetTransferStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `IdKind` | `TransactionIdKind` | yes | ID kind. Moolre field `idtype`. |
| `Id` | `string` | yes | ID. Moolre field `id`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The transaction.

| Throws | When |
| --- | --- |
| `ArgumentException` | The ID is missing or the ID kind is not set. |
| `InvalidOperationException` | No account number is set on the request or in the options, or a credential is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `SendAsync`

```csharp
Task<TransferResult> moolre.Transfers.SendAsync(SendTransferRequest request, CancellationToken cancellationToken = default)
```

Sends money to a mobile money number or a bank account. **Moves money**: never retried. Use a new, unique `ExternalReference` for each transfer, and keep it.

| Parameter | Description |
| --- | --- |
| `request` | The transfer. |
| `cancellationToken` | Cancels the call. Cancelling after the transfer started gives `MoolreOutcomeUncertainException`, because it may have gone through. |

<details><summary><code>SendTransferRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Channel` | `TransferChannel` | yes | Channel. Moolre field `channel`. |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `Amount` | `decimal` | yes | Amount. Moolre field `amount` (string). |
| `Receiver` | `string` | yes | Receiver. Moolre field `receiver` (mobile number starting with 0, or bank account number). |
| `BankCode` | `string?` |  | Bank code. Moolre field `sublistid` (required for Bank). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `Narration` | `string?` |  | Narration. Moolre field `reference`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The transfer as Moolre reports it. Check `Status`: `Pending` means confirm later with `GetStatusAsync`.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid (for example an amount that is not above zero). |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. Do not resend; check the original. |
| `MoolreOutcomeUncertainException` | The transfer may or may not have happened. Check its status with the same reference. |
| `MoolreApiException` | Moolre refused the transfer (for example insufficient balance). No money moved. |

### `SendInternalAsync`

```csharp
Task<InternalTransferResult> moolre.Transfers.SendInternalAsync(SendInternalTransferRequest request, CancellationToken cancellationToken = default)
```

Moves money from your wallet to another Moolre wallet. **Moves money**: never retried. Moolre only confirms that the transfer started; check the final state with `GetStatusAsync`.

| Parameter | Description |
| --- | --- |
| `request` | The transfer. |
| `cancellationToken` | Cancels the call. Cancelling after the transfer started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendInternalTransferRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `Amount` | `decimal` | yes | Amount. Moolre field `amount` (string). |
| `ReceiverAccountNumber` | `string` | yes | Receiver account number. Moolre field `receiver` (a Moolre account number, not a phone). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (required, unique). |
| `Narration` | `string?` |  | Narration. Moolre field `reference`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The ID Moolre gave the transfer.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. |
| `MoolreOutcomeUncertainException` | The transfer may or may not have happened. |
| `MoolreApiException` | Moolre refused the transfer (for example insufficient balance). No money moved. |

### `ValidateNameAsync`

```csharp
Task<NameValidationResult> moolre.Transfers.ValidateNameAsync(ValidateNameRequest request, CancellationToken cancellationToken = default)
```

Looks up the name on a mobile money or bank account before you send money to it. Safe to repeat. A number that is not found is a normal result (`Found` is `false`), not an exception.

| Parameter | Description |
| --- | --- |
| `request` | The account to check. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>ValidateNameRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Receiver` | `string` | yes | Receiver. Moolre field `receiver` (phone or bank account number). |
| `Channel` | `TransferChannel` | yes | Channel. Moolre field `channel`. |
| `BankCode` | `string?` |  | Bank code. Moolre field `sublistid` (required when Channel is Bank). |
| `Currency` | `Currency` | yes | Currency. Moolre field `currency`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The account holder's name, or a not-found result.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing, or the bank code is missing for a bank account. |
| `InvalidOperationException` | No account number is set on the request or in the options, or a credential is missing. |
| `MoolreAuthenticationException` | Moolre rejected the credentials. |
| `MoolreApiException` | Moolre reported another failure. |

## Accounts

`moolre.Accounts`. Uses the private key.

### `CreateSandboxAccountAsync`

```csharp
Task<AccountResult> moolre.Accounts.CreateSandboxAccountAsync(CreateSandboxAccountRequest request, CancellationToken cancellationToken = default)
```

Creates a wallet in the sandbox, for testing. Moolre only offers this in the sandbox; live wallets are created in the Moolre app. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The wallet. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>CreateSandboxAccountRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountName` | `string` | yes | Account name. Moolre field `accountname`. |
| `Currency` | `Currency` |  | Currency. Moolre field `currency` (GHS only). |
| `ApiEnabled` | `bool?` |  | API enabled. Moolre field `api`. |
| `CallbackUrl` | `Uri` | yes | Callback URL. Moolre field `callback`. |
| `Settlement` | `SettlementSettings` | yes | Settlement. Moolre field `settlement`. |

</details>

**Returns:** The new wallet, including its account number and callback secret (sensitive: store it securely, never log it).

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid (the currency must be GHS). |
| `InvalidOperationException` | The SDK is set to the live environment, or the API user is missing. |
| `MoolreOutcomeUncertainException` | The wallet may or may not have been created. |
| `MoolreApiException` | Moolre refused it (for example `APGW21`, the account name is already used). |

### `GetStatusAsync`

```csharp
Task<AccountStatusResult> moolre.Accounts.GetStatusAsync()
```

Gets the balance and settings of the default wallet from `AccountNumber`.

**Returns:** The wallet's balance, name, activation state and callback URL.

### `GetStatusAsync`

```csharp
Task<AccountStatusResult> moolre.Accounts.GetStatusAsync(CancellationToken cancellationToken)
```

Gets the balance and settings of the default wallet from `AccountNumber`.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The wallet's balance, name, activation state and callback URL.

### `GetStatusAsync`

```csharp
Task<AccountStatusResult> moolre.Accounts.GetStatusAsync(GetAccountStatusRequest request, CancellationToken cancellationToken = default)
```

Gets the balance and settings of a wallet.

| Parameter | Description |
| --- | --- |
| `request` | Which wallet; the default wallet from `AccountNumber` when its account number is omitted. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetAccountStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The wallet's balance, name, activation state and callback URL.

| Throws | When |
| --- | --- |
| `InvalidOperationException` | No account number is set on the request or in the options. |
| `MoolreAuthenticationException` | Moolre rejected the credentials or the wallet. |

### `ListTransactionsAsync`

```csharp
Task<AccountTransactionsResult> moolre.Accounts.ListTransactionsAsync()
```

Lists the default wallet's transactions, with no filters. Safe to repeat.

**Returns:** The transactions.

### `ListTransactionsAsync`

```csharp
Task<AccountTransactionsResult> moolre.Accounts.ListTransactionsAsync(CancellationToken cancellationToken)
```

Lists the default wallet's transactions, with no filters. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The transactions.

### `ListTransactionsAsync`

```csharp
Task<AccountTransactionsResult> moolre.Accounts.ListTransactionsAsync(ListAccountTransactionsRequest request, CancellationToken cancellationToken = default)
```

Lists a wallet's transactions, optionally filtered by date and status. Safe to repeat. No transactions is an empty list.

| Parameter | Description |
| --- | --- |
| `request` | The wallet and filters; the default wallet when its account number is omitted. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>ListAccountTransactionsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |
| `From` | `DateTimeOffset?` |  | From. Moolre field `startdate` "YYYY-MM-DD HH:MM:SS". |
| `To` | `DateTimeOffset?` |  | To. Moolre field `enddate` (defaults to end of current year). |
| `Limit` | `int?` |  | Limit. Moolre field `limit` (sent as a string). |
| `Status` | `TransactionStatus?` |  | Status. Moolre field `status` filter 0 / 1 / 2. |

</details>

**Returns:** The transactions.

| Throws | When |
| --- | --- |
| `ArgumentException` | The date range is reversed, the limit is not above zero, or the status is not a known value. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `UpdateAsync`

```csharp
Task<AccountResult> moolre.Accounts.UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default)
```

Changes a wallet's name, callback URL, API access or settlement. Only the values you set are sent. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The changes; the wallet is the default from `AccountNumber` when its account number is omitted. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>UpdateAccountRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |
| `AccountName` | `string?` |  | Account name. Moolre field `accountname`. |
| `ApiEnabled` | `bool?` |  | API enabled. Moolre field `api` (send only to change API access). |
| `CallbackUrl` | `Uri?` |  | Callback URL. Moolre field `callback`. |
| `Settlement` | `SettlementSettings?` |  | Settlement. Moolre field `settlement`. |

</details>

**Returns:** The wallet after the change.

| Throws | When |
| --- | --- |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreOutcomeUncertainException` | The change may or may not have been applied. |
| `MoolreApiException` | Moolre refused it (for example `WCU01`, invalid account number). |

## Sales

`moolre.Sales`. Uses the private key. The wallet must be linked to Moolre's Sell on Our Platform service.

### `GetStatusAsync`

```csharp
Task<SaleStatusResult> moolre.Sales.GetStatusAsync(GetSaleStatusRequest request, CancellationToken cancellationToken = default)
```

Gets where a sale stands: sent, pending, failed and refunded, or not found. Safe to repeat. Every one of these is a normal result.

| Parameter | Description |
| --- | --- |
| `request` | Which sale, by your external reference, Moolre's transaction ID or the sale reference. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetSaleStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `IdKind` | `SaleIdKind` | yes | ID kind. Moolre field `idtype`. |
| `Id` | `string` | yes | ID. Moolre field `id`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The sale's status and details.

| Throws | When |
| --- | --- |
| `ArgumentException` | The ID is missing or the ID kind is not set. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreApiException` | Moolre could not answer (for example `ASS08`, unknown error). Do not assume the sale failed. |

### `ListBundlesAsync`

```csharp
Task<BundleCatalogResult> moolre.Sales.ListBundlesAsync()
```

Lists the data bundles for every network, for the default wallet. Safe to repeat.

**Returns:** The bundles for MTN, Telecel and AT.

### `ListBundlesAsync`

```csharp
Task<BundleCatalogResult> moolre.Sales.ListBundlesAsync(CancellationToken cancellationToken)
```

Lists the data bundles for every network, for the default wallet. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The bundles for MTN, Telecel and AT.

### `ListBundlesAsync`

```csharp
Task<BundleCatalogResult> moolre.Sales.ListBundlesAsync(ListBundlesRequest request, CancellationToken cancellationToken = default)
```

Lists the data bundles Moolre sells, per network. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | The wallet and an optional network; all networks when the network is omitted. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>ListBundlesRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |
| `Network` | `SalesNetwork?` |  | Network. Moolre field `network` (omit for all networks). |

</details>

**Returns:** The bundles for MTN, Telecel and AT (an empty list for a network with none).

| Throws | When |
| --- | --- |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreApiException` | Moolre reported a failure. |

### `SendAirtimeAsync`

```csharp
Task<SaleResult> moolre.Sales.SendAirtimeAsync(SendAirtimeRequest request, CancellationToken cancellationToken = default)
```

Sends airtime to a phone. **Spends money from the wallet**: never retried. Use a new, unique `ExternalReference` for each sale, and keep it.

| Parameter | Description |
| --- | --- |
| `request` | The sale. Moolre's minimum is GHS 1.00. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendAirtimeRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Network` | `SalesNetwork` | yes | Network. Moolre field `channel`. |
| `ReceiverPhoneNumber` | `string` | yes | Receiver phone number. Moolre field `receiver`. |
| `Amount` | `decimal` | yes | Amount. Moolre field `amount` (string, GHS, minimum 1.00 — enforced by Moolre). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (unique per wallet). |
| `Narration` | `string?` |  | Narration. Moolre field `reference`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber` (must be linked to the Sell on Our Platform service). |

</details>

**Returns:** The sale. `IsInProgress` means not delivered yet; confirm with `GetStatusAsync`.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. |
| `MoolreOutcomeUncertainException` | The sale may or may not have gone through (including Moolre's `ASL06`, unknown error). Check its status. |
| `MoolreApiException` | Moolre refused the sale (for example an amount under GHS 1.00). Nothing was spent. |

### `SendDataAsync`

```csharp
Task<SaleResult> moolre.Sales.SendDataAsync(SendDataRequest request, CancellationToken cancellationToken = default)
```

Sends a data bundle to a phone. **Spends money from the wallet**: never retried. The bundle's own price is charged, so no amount is sent. Use a new, unique `ExternalReference` for each sale.

| Parameter | Description |
| --- | --- |
| `request` | The sale; the bundle ID comes from `ListBundlesAsync`. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendDataRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Network` | `SalesNetwork` | yes | Network. Moolre field `channel`. |
| `ReceiverPhoneNumber` | `string` | yes | Receiver phone number. Moolre field `receiver`. |
| `BundleId` | `string` | yes | Bundle ID. Moolre field `bundle` (an id returned by ListBundlesAsync, passed unchanged). |
| `ExternalReference` | `string` | yes | External reference. Moolre field `externalref` (unique per wallet). |
| `Narration` | `string?` |  | Narration. Moolre field `reference`. |
| `AccountNumber` | `string?` |  | Account number. Moolre field `accountnumber`. |

</details>

**Returns:** The sale. `IsInProgress` means not delivered yet; confirm with `GetStatusAsync`.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing or invalid. |
| `InvalidOperationException` | No account number is set on the request or in the options, or the private key is missing. |
| `MoolreDuplicateReferenceException` | The external reference was already used. |
| `MoolreOutcomeUncertainException` | The sale may or may not have gone through. Check its status. |
| `MoolreApiException` | Moolre refused the sale (for example a bundle not sold on that network). Nothing was spent. |

## SMS

`moolre.Sms`. Uses only the SMS VAS key.

### `ApproveSenderIdsAsync`

```csharp
Task<SenderIdListResult> moolre.Sms.ApproveSenderIdsAsync(ApproveSenderIdsRequest request, CancellationToken cancellationToken = default)
```

Approves, rejects or resets sender IDs. Needs administrative permission on the SMS service. Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The decisions. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>ApproveSenderIdsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Decisions` | `Collection<SenderIdDecision>` |  | Decisions. Moolre field `senderids`. |

</details>

**Returns:** The sender IDs after the change.

| Throws | When |
| --- | --- |
| `ArgumentException` | No decisions, or a blank sender ID. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreOutcomeUncertainException` | The change may or may not have been applied. |
| `MoolreApiException` | Moolre refused it (for example `ASMQ09`, no permission). |

### `CreateSenderIdsAsync`

```csharp
Task<SenderIdsCreatedResult> moolre.Sms.CreateSenderIdsAsync(CreateSenderIdsRequest request, CancellationToken cancellationToken = default)
```

Asks Moolre to register sender IDs (at most 11 characters, letters and digits, not only digits). Not retried automatically.

| Parameter | Description |
| --- | --- |
| `request` | The sender IDs. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>CreateSenderIdsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `SenderIds` | `Collection<SenderIdRegistration>` |  | Sender ids. Moolre field `senderids`. |

</details>

**Returns:** Moolre's acceptance (`ASMQ12`). Approval comes later; check it with `GetSenderIdStatusAsync`.

| Throws | When |
| --- | --- |
| `ArgumentException` | No sender IDs, a blank one, or one longer than 11 characters. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreOutcomeUncertainException` | The request may or may not have reached Moolre. |
| `MoolreApiException` | Moolre refused it (for example `ASMQ09`, no permission to approve). |

### `GetBalanceAsync`

```csharp
Task<SmsBalanceResult> moolre.Sms.GetBalanceAsync()
```

Gets the SMS credit balance. Safe to repeat.

**Returns:** The balance.

### `GetBalanceAsync`

```csharp
Task<SmsBalanceResult> moolre.Sms.GetBalanceAsync(CancellationToken cancellationToken)
```

Gets the SMS credit balance. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The balance.

| Throws | When |
| --- | --- |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `GetSenderIdStatusAsync`

```csharp
Task<SenderIdStatusResult> moolre.Sms.GetSenderIdStatusAsync(GetSenderIdStatusRequest request, CancellationToken cancellationToken = default)
```

Gets whether a sender ID is approved, pending or rejected. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | The sender ID. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetSenderIdStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `SenderId` | `string` | yes | Sender ID. Moolre field `senderid`. |

</details>

**Returns:** The sender ID's approval and whitelisting.

| Throws | When |
| --- | --- |
| `ArgumentException` | The sender ID is missing. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `GetStatusAsync`

```csharp
Task<SmsStatusResult> moolre.Sms.GetStatusAsync(GetSmsStatusRequest request, CancellationToken cancellationToken = default)
```

Gets the delivery status of messages by the references you gave them. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | The message references. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetSmsStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `References` | `Collection<string>` |  | References. Moolre field `ref` (array of message references). |

</details>

**Returns:** One status per reference Moolre knows.

| Throws | When |
| --- | --- |
| `ArgumentException` | No references, or a blank one. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `ListSenderIdsAsync`

```csharp
Task<SenderIdListResult> moolre.Sms.ListSenderIdsAsync()
```

Lists your sender IDs and their approval. Safe to repeat.

**Returns:** The sender IDs.

### `ListSenderIdsAsync`

```csharp
Task<SenderIdListResult> moolre.Sms.ListSenderIdsAsync(CancellationToken cancellationToken)
```

Lists your sender IDs and their approval. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The sender IDs.

| Throws | When |
| --- | --- |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `SendAsync`

```csharp
Task<SmsSendResult> moolre.Sms.SendAsync(SendSmsRequest request, CancellationToken cancellationToken = default)
```

Sends one or more SMS from an approved sender ID. **Sends messages and spends SMS credit**: never retried. Give each message a `Reference` if you want to look up its delivery later with `GetStatusAsync`.

| Parameter | Description |
| --- | --- |
| `request` | The sender ID and the messages. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendSmsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `SenderId` | `string` | yes | Sender ID. Moolre field `senderid` (max 11 characters, must be approved). |
| `Network` | `SmsDeliveryNetwork` |  | Network. Moolre field `network` (live only). |
| `Messages` | `Collection<SmsMessage>` |  | Messages. Moolre field `messages` (initialised empty). |

</details>

**Returns:** Moolre's acceptance (`SMS01`). Delivery is reported per message by `GetStatusAsync`.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing, a sender ID is longer than 11 characters, or there are no messages. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreOutcomeUncertainException` | The messages may or may not have been sent. Check their status before resending. |
| `MoolreApiException` | Moolre refused the send (for example `ASMS07`, the sender ID is not approved). Nothing was sent. |

### `SendSingleAsync`

```csharp
Task<SmsSendResult> moolre.Sms.SendSingleAsync(SendSingleSmsRequest request, CancellationToken cancellationToken = default)
```

Sends one SMS to one recipient with Moolre's GET form (at most 160 characters). **Sends a message**: never retried. Moolre puts the message text in the URL for this call, so it can appear in web server logs; prefer `SendAsync` for anything sensitive.

| Parameter | Description |
| --- | --- |
| `request` | The sender ID, recipient and text. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendSingleSmsRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `SenderId` | `string` | yes | Sender ID. Moolre field `senderid`. |
| `Network` | `SmsDeliveryNetwork` |  | Network. Moolre field `network`. |
| `Recipient` | `string` | yes | Recipient. Moolre field `recipient`. |
| `Text` | `string` | yes | Text. Moolre field `message` (max 160 characters). |

</details>

**Returns:** Moolre's acceptance (`SMS01`).

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing, the sender ID is longer than 11 characters, or the text is longer than 160. |
| `InvalidOperationException` | The SMS key is not set. |
| `MoolreOutcomeUncertainException` | The message may or may not have been sent. |
| `MoolreApiException` | Moolre refused the send. Nothing was sent. |

## WhatsApp

`moolre.WhatsApp`. Uses only the WhatsApp VAS key.

### `GetAllTemplatesAsync`

```csharp
IAsyncEnumerable<WhatsAppTemplate> moolre.WhatsApp.GetAllTemplatesAsync(CancellationToken cancellationToken = default)
```

Goes through every template, page by page, fetching the next page only when you reach it (nothing is collected into one big list).

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the walk between and during pages. |

**Returns:** Every template, in Moolre's order.

| Throws | When |
| --- | --- |
| `MoolreResponseException` | Moolre returned a cursor it had already returned, which would loop forever. |

### `GetStatusAsync`

```csharp
Task<WhatsAppStatusResult> moolre.WhatsApp.GetStatusAsync(GetWhatsAppMessageStatusRequest request, CancellationToken cancellationToken = default)
```

Gets the delivery status of messages by their references. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | The message references. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetWhatsAppMessageStatusRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `References` | `Collection<string>` |  | References. Moolre field `ref` (array). |

</details>

**Returns:** One status per reference Moolre knows (for example `accepted` or `read`).

| Throws | When |
| --- | --- |
| `ArgumentException` | No references, or a blank one. |
| `InvalidOperationException` | The WhatsApp key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `GetTemplatePageAsync`

```csharp
Task<WhatsAppTemplatesPage> moolre.WhatsApp.GetTemplatePageAsync()
```

Gets the first page of your WhatsApp templates. Safe to repeat.

**Returns:** The first page and the cursor for the next.

### `GetTemplatePageAsync`

```csharp
Task<WhatsAppTemplatesPage> moolre.WhatsApp.GetTemplatePageAsync(CancellationToken cancellationToken)
```

Gets the first page of your WhatsApp templates. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `cancellationToken` | Cancels the call. |

**Returns:** The first page and the cursor for the next.

### `GetTemplatePageAsync`

```csharp
Task<WhatsAppTemplatesPage> moolre.WhatsApp.GetTemplatePageAsync(GetWhatsAppTemplatesRequest request, CancellationToken cancellationToken = default)
```

Gets one page of your WhatsApp templates. Safe to repeat.

| Parameter | Description |
| --- | --- |
| `request` | The cursor from the previous page, or none for the first page. |
| `cancellationToken` | Cancels the call. |

<details><summary><code>GetWhatsAppTemplatesRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `Cursor` | `string?` |  | Cursor. Moolre field `next` (opaque; omit for the first page). |

</details>

**Returns:** The templates on this page and the cursor for the next (`null` on the last page).

| Throws | When |
| --- | --- |
| `InvalidOperationException` | The WhatsApp key is not set. |
| `MoolreApiException` | Moolre reported a failure. |

### `SendAsync`

```csharp
Task<WhatsAppSendResult> moolre.WhatsApp.SendAsync(SendWhatsAppMessagesRequest request, CancellationToken cancellationToken = default)
```

Sends a template message to one or more recipients. **Sends messages and spends WhatsApp credit**: never retried. Give each message a unique `Reference` to look up its delivery later.

| Parameter | Description |
| --- | --- |
| `request` | The approved template, its language, and the messages. |
| `cancellationToken` | Cancels the call. Cancelling after it started gives `MoolreOutcomeUncertainException`. |

<details><summary><code>SendWhatsAppMessagesRequest</code> fields</summary>

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `TemplateName` | `string` | yes | Template name. Moolre field `template_name` (an approved template). |
| `Language` | `string` | yes | Language. Moolre field `language` (English only). |
| `Messages` | `Collection<WhatsAppMessage>` |  | Messages. Moolre field `messages`. |

</details>

**Returns:** The result. Check `AllSent`: Moolre accepts the call but skips messages whose reference was not unique, and lists them in `NotSent`.

| Throws | When |
| --- | --- |
| `ArgumentException` | A required value is missing, or there are no messages. |
| `InvalidOperationException` | The WhatsApp key is not set. |
| `MoolreOutcomeUncertainException` | The messages may or may not have been sent. Check their status before resending. |
| `MoolreApiException` | Moolre refused the send (for example `WAS401`, insufficient balance). Nothing was sent. |

## Reference data

`moolre.ReferenceData`. Needs no credentials.

### `GetBanksAsync`

```csharp
Task<BankListResult> moolre.ReferenceData.GetBanksAsync(string countryCode, CancellationToken cancellationToken = default)
```

Gets the banks Moolre supports in a country.

| Parameter | Description |
| --- | --- |
| `countryCode` | Moolre's country code, for example `gha`. |
| `cancellationToken` | Cancels the call. |

**Returns:** The banks. Use a bank's `Code` as the bank code in requests.

| Throws | When |
| --- | --- |
| `ArgumentException` | `countryCode` is null, empty or white space. |

## Setup

### `AddMoolre`

```csharp
IServiceCollection services.AddMoolre(Action<MoolreOptions> configure)
```

Registers one wallet as the default `MoolreClient`. Inject `MoolreClient` (or one of its areas, such as `MoolrePayments`) to use it. The settings are checked when the application starts. The SDK sends through a named `HttpClient` (`Zhuwa.Moolre`), so you can add handlers to it with `services.AddHttpClient("Zhuwa.Moolre")`. Do not retry `POST` requests blindly: every Moolre call is a POST.

| Parameter | Description |
| --- | --- |
| `services` | The service collection. |
| `configure` | Sets the environment, credentials and wallet. Read secrets from configuration or a secret store. |

**Returns:** The same service collection.

### `AddMoolre`

```csharp
IServiceCollection services.AddMoolre(string name, Action<MoolreOptions> configure)
```

Registers a named wallet. Moolre issues credentials per wallet (a key for one wallet cannot act on another), so a business with several wallets registers each one by name with its own settings, then injects `MoolreClientFactory` and calls `Create(name)`. The settings are checked when the application starts. Each wallet sends through its own named `HttpClient`, `Zhuwa.Moolre:<name>`.

| Parameter | Description |
| --- | --- |
| `services` | The service collection. |
| `name` | The wallet's name, used with `Create`. |
| `configure` | Sets this wallet's environment, credentials and account number. |

**Returns:** The same service collection.

| Throws | When |
| --- | --- |
| `ArgumentException` | `name` is null, empty or white space. |

### `AddMoolreWebhooks`

```csharp
IServiceCollection services.AddMoolreWebhooks(Action<MoolreWebhookOptions> configure)
```

Registers `MoolreWebhookVerifier` for your payment callback endpoint. The secret is checked when the application starts.

| Parameter | Description |
| --- | --- |
| `services` | The service collection. |
| `configure` | Sets the callback secret (`Secret`) or one per wallet (`WalletSecrets`). Read them from configuration or a secret store. |

**Returns:** The same service collection.

### `Create`

```csharp
MoolreClient factory.Create(string name)
```

Gets the client for a registered wallet. The same name always gives the same client, which is safe to share.

| Parameter | Description |
| --- | --- |
| `name` | The name the wallet was registered with. |

**Returns:** A client that sends every call with that wallet's credentials and settings.

| Throws | When |
| --- | --- |
| `ArgumentNullException` | `name` is null. |
| `InvalidOperationException` | No wallet is registered with that name. |
| `OptionsValidationException` | The wallet's settings are invalid. |

## Webhooks

### `Parse`

```csharp
PaymentWebhook PaymentWebhook.Parse(ReadOnlySpan<byte> utf8Json)
```

Reads the body Moolre posted to your callback URL. Parsing does not verify it: check the secret with `IsSecretValid` before changing any records.

| Parameter | Description |
| --- | --- |
| `utf8Json` | The raw request body. |

**Returns:** The callback.

| Throws | When |
| --- | --- |
| `MoolreResponseException` | The body is not a Moolre payment callback. |

### `Parse`

```csharp
PaymentWebhook PaymentWebhook.Parse(string json)
```

Reads the body Moolre posted to your callback URL, given as text. See `Parse`.

| Parameter | Description |
| --- | --- |
| `json` | The raw request body. |

**Returns:** The callback.

| Throws | When |
| --- | --- |
| `ArgumentNullException` | `json` is null. |
| `MoolreResponseException` | The body is not a Moolre payment callback. |

### `TryParse`

```csharp
bool PaymentWebhook.TryParse(ReadOnlySpan<byte> utf8Json, out PaymentWebhook? webhook)
```

Tries to read a callback body; returns `false` instead of throwing when it is not a Moolre callback.

| Parameter | Description |
| --- | --- |
| `utf8Json` | The raw request body. |
| `webhook` | The callback, when the body could be read. |

**Returns:** Whether the body could be read.

### `TryParse`

```csharp
bool PaymentWebhook.TryParse(string? json, out PaymentWebhook? webhook)
```

Tries to read a callback body given as text. See `TryParse`.

| Parameter | Description |
| --- | --- |
| `json` | The raw request body; `null` gives `false`. |
| `webhook` | The callback, when the body could be read. |

**Returns:** Whether the body could be read.

### `IsSecretValid`

```csharp
bool verifier.IsSecretValid(PaymentWebhook webhook)
```

Whether the callback carries the right secret for its wallet: the one in `WalletSecrets` for the callback's account number, or `Secret` otherwise. A callback for a wallet with no secret configured is rejected. The comparison takes the same time whatever the input, so timing cannot reveal a secret. Reject the callback when this is `false`.

| Parameter | Description |
| --- | --- |
| `webhook` | The parsed callback. |

**Returns:** `true` only when the secrets match exactly.

| Throws | When |
| --- | --- |
| `ArgumentNullException` | `webhook` is null. |
| `InvalidOperationException` | No secret is configured at all; a missing setting never counts as a match. |
