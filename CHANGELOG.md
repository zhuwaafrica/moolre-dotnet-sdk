# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

The other 29 operations, `MoolreClientFactory`, `MoolreModelFactory`, the webhook parser and verifier are not implemented yet.
