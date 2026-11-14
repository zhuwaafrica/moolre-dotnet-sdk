# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Project setup: `net10.0` library `Zhuwa.Moolre.Sdk`, xunit v3 test project, central package management, pinned SDK, strict analyzers (warnings are errors), documentation file generation, trimming and AOT analyzers, package validation, Source Link and symbol packages, NuGet vulnerability audit.
- Public API tracking (`.PublicAPI/Shipped.txt` and `Unshipped.txt`) and a banned-APIs list (`BannedSymbols.txt`), both enforced by the build.
- One `artifacts/` output folder, a separate NuGet README (`package-readme.md`), Dependabot, dependency review, issue and pull request templates, Code of Conduct, and GitHub Actions pinned to commit SHAs.
- Moolre's documented example responses as test fixtures (`tests/Zhuwa.Moolre.Sdk.Tests/Fixtures/`).
- Each API area is organised into `Requests/`, `Results/` and `Models/`.

No Moolre endpoints are implemented yet.
