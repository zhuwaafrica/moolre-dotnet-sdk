# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-11-17

### Added

- Webhook signature verification: `MoolreWebhookVerifier.IsSignatureValid` checks Moolre's `X-Moolre-Signature` header (HMAC-SHA256 of `"<t>.<raw body>"` with the wallet's callback secret, constant-time, 5-minute `MoolreWebhookOptions.SignatureTolerance`), tested against Moolre's published vectors. `SignatureHeaderName` names the header.
- `PaymentWebhook.EventId` and `PaymentWebhook.EventType` (Moolre's `event_id` and `event`), for handling each event once.

### Deprecated

- `MoolreWebhookVerifier.IsSecretValid` and `PaymentWebhookData.Secret`: Moolre removes `data.secret` after 31 March 2027. Use `IsSignatureValid`.

### Fixed

- `CreateSandboxAccountAsync` now sends only `X-API-USER`. Moolre documents the key as optional there, and a live private key made the sandbox answer AIN01 (found in a sandbox run).

- Disabled automatic cookies in the SDK HTTP handler and removed remote response codes and raw transport exceptions from SDK logs.
- Validate the full supported timeout range during options validation.

### Changed

- Reuse the JSON request buffer and map array responses without an intermediate list.

### Added

- Project setup: `net10.0` library `Zhuwa.Moolre.Sdk`, xunit v3 test project, central package management, pinned SDK, strict analyzers (warnings are errors), documentation file generation, trimming and AOT analyzers, package validation, Source Link and symbol packages, NuGet vulnerability audit.
- Public API tracking (`.PublicAPI/Shipped.txt` and `Unshipped.txt`) and a banned-APIs list (`BannedSymbols.txt`), both enforced by the build.
- One `artifacts/` output folder, a separate NuGet README (`package-readme.md`), Dependabot, dependency review, issue and pull request templates, Code of Conduct, and GitHub Actions pinned to commit SHAs.
- Moolre's documented example responses as test fixtures (`tests/Zhuwa.Moolre.Sdk.Tests/Fixtures/`).
- Each API area is organised into `Requests/`, `Results/` and `Models/`.

- The public data types: 25 request types, 25 result types, 18 shared models, the enums, `MoolreOptions`, the extensible code structs (`MoolreResponseCode` with every documented Moolre code, `SenderIdApproval`, `WhatsAppTemplateStatus`, `WhatsAppDeliveryStatus`) and the webhook payload types. Results are immutable; their constructors are internal.
- The endpoint table (`MoolreEndpoints`): every Moolre path, HTTP method, `type` selector, credential kind and whether the call is safe to repeat, in one place.

- The call pipeline: the exception types (`MoolreException`, `MoolreApiException`, `MoolreAuthenticationException`, `MoolreDuplicateReferenceException`, `MoolreResponseException`, `MoolreOutcomeUncertainException`), tolerant response parsing, credential headers per operation, the timeout, and `AddMoolre`. Calls that move money or send messages are never retried and report an uncertain outcome with the caller's reference.
- Two working operations so far: `Accounts.GetStatusAsync` and `ReferenceData.GetBanksAsync`.
- Transfers: `ValidateNameAsync`, `SendAsync`, `GetStatusAsync` and `SendInternalAsync`. Amounts must be above zero with at most two decimals; a missing transfer status reads as pending, never successful; "pay out in progress" is a pending result; insufficient balance and payout failures throw `MoolreApiException`.
- Payments: `RequestMobileMoneyAsync` (prompt sent, OTP required and unknown codes as distinct outcomes; `SkipOtp` refused outside the sandbox), `GetStatusAsync` (with its own `GetPaymentStatusRequest`), `CreatePaymentLinkAsync` (expiry in whole minutes, metadata), `CreatePaymentIdAsync` (international phone format), `CreateBankAccountAsync`, `ListBankAccountsAsync` and `GetPaymentAccountAsync` (not found is a result; one or many transactions).
- Webhooks: `PaymentWebhook.Parse` / `TryParse` (bytes or text) for Moolre's payment callbacks, `MoolreWebhookVerifier` (constant-time secret check; Moolre's callback addresses are listed in the README for firewall rules, not hard-coded) and `AddMoolreWebhooks`, which fails at startup without a secret.
- Accounts: `CreateSandboxAccountAsync` (sandbox only, refused in live), `UpdateAsync` (sends only the values set) and `ListTransactionsAsync` (GMT date filters, "no transactions" is an empty list).
- Sales: `SendAirtimeAsync` and `SendDataAsync` ("in progress" is a pending result; Moolre's `ASL06` unknown error is reported as an uncertain outcome with the reference, never as a failure; GHS always sent; no amount on data sales), `ListBundlesAsync` (per network) and `GetStatusAsync` (sent, pending, failed and refunded, not found are all results).
- SMS: `SendAsync` (bulk), `SendSingleAsync` (GET form, 160 characters, type in the query string), `GetStatusAsync`, `CreateSenderIdsAsync`, `GetSenderIdStatusAsync`, `ListSenderIdsAsync`, `ApproveSenderIdsAsync` and `GetBalanceAsync`, all with the SMS key only. Sender IDs are checked against Moolre's 11-character limit; sends are never retried.
- GET calls with a `type` selector now carry it in the query string.

- WhatsApp: `GetTemplatePageAsync`, `GetAllTemplatesAsync` (walks every page by cursor, one page at a time, and stops on a repeated cursor instead of looping), `SendAsync` (reports messages skipped for duplicate references; never retried) and `GetStatusAsync`, all with the WhatsApp key only.
- Several wallets: `AddMoolre(name, options => ...)` registers a wallet by name with its own credentials (Moolre issues keys per wallet), and `MoolreClientFactory.Create(name)` returns its client. Each wallet has its own settings, startup validation and `HttpClient` (`Zhuwa.Moolre:<name>`); an unknown name throws a clear error. The single-wallet `AddMoolre(options => ...)` is unchanged.
- Per-wallet callback secrets: `MoolreWebhookOptions.WalletSecrets` (keyed by wallet account number) lets one endpoint verify callbacks for several wallets; each callback is checked against its own wallet's secret, with `Secret` as an optional fallback, and a callback for a wallet with no secret is rejected.
- User guides in `docs/`: getting started, configuration, payments, transfers, accounts, sales, SMS, WhatsApp, webhooks, errors and money safety, testing.
- Tests: about 170 more cases (every operation's credentials and method, server cultures with decimal commas, oversized and error responses, every converter, exceptions, value types, base-class defaults, concurrent wallet access); hand-written code at about 97% line coverage.
- Fixed: the 16 MB response limit was not enforced; it is now. Fixed: on Linux and macOS a path such as `/callback` was accepted as a `file://` URL; URLs must now be `http` or `https`.
- Documentation: the README is now a user guide (requirements, installation, usage, several wallets, exception handling, retries and timeouts, webhooks); design diagrams moved to `docs/architecture.md`; new `docs/reference.md` lists every method with its signature, parameters, request fields and exceptions, generated from the code by `eng/generate-reference.py`.
- Package: removed the unused `Microsoft.Extensions.Options.ConfigurationExtensions` dependency; the NuGet page now shows real usage, and the release notes point to this changelog.

All 31 documented operations are implemented. Not yet done: verification against the real sandbox, `MoolreModelFactory`, configuration binding, samples.

[Unreleased]: https://github.com/zhuwaafrica/moolre-dotnet-sdk/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/zhuwaafrica/moolre-dotnet-sdk/releases/tag/v1.0.0
