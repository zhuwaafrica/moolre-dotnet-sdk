# Moolre .NET SDK

An **unofficial, community-built** .NET client for the [Moolre](https://docs.moolre.com/) API, maintained by [Zhuwa Africa](https://github.com/zhuwaafrica). NuGet package: `Zhuwa.Moolre.Sdk`.

> This project is independent. It is not affiliated with, endorsed by, or supported by Moolre.

**Status: preview.** The project is set up, builds and tests, but no Moolre endpoint is implemented yet. The NuGet page text is in [`package-readme.md`](package-readme.md).

## Repository layout

| Path | Contents |
| --- | --- |
| `src/Zhuwa.Moolre.Sdk/` | The library (`net10.0`). One folder per API area (`Accounts`, `Transfers`, `Payments`, `Sales`, `Sms`, `WhatsApp`, `Reference`); inside each, `Requests/`, `Results/` and `Models/`, with the client and enums at the folder root. Shared core types live in `Clients/`, `Configuration/`, `Enums/`, `Requests/`, `Results/` and `Models/`. Also `Errors/`, `Webhooks/`, `DependencyInjection/`, and `Internal/` (never public API) |
| `src/Zhuwa.Moolre.Sdk/.PublicAPI/` | The reviewed public API surface (`Shipped` and `Unshipped`); an unlisted public change fails the build |
| `tests/Zhuwa.Moolre.Sdk.Tests/` | xunit v3 tests mirroring the source layout; `Fixtures/` holds Moolre's documented example responses; no network, no credentials |
| `samples/` | Sample applications (added with the code they exercise) |
| `Directory.Build.props`, `Directory.Packages.props`, `BannedSymbols.txt`, `.editorconfig`, `global.json` | Shared build settings, central package versions, banned APIs, analyzer rules, pinned SDK |

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack src/Zhuwa.Moolre.Sdk -c Release
```

The .NET SDK is pinned in `global.json`. Build output goes to `artifacts/`. Warnings are errors; see [CONTRIBUTING.md](CONTRIBUTING.md).

## More

- [Troubleshooting](TROUBLESHOOTING.md)
- [Changelog](CHANGELOG.md)
- [Contributing](CONTRIBUTING.md), [Security policy](SECURITY.md) and [Code of Conduct](CODE_OF_CONDUCT.md)

## License

[MIT](LICENSE)
