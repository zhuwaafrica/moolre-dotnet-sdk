# Contributing

Thanks for helping. This is an unofficial community SDK for the Moolre API, maintained by Zhuwa Africa.

## Before you start

- For a new feature or a public API change, open an issue first so the design can be agreed.
- Moolre's own documentation is the source of truth for request and response shapes. If it does not say something, say so in the issue rather than guessing.

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The .NET SDK is pinned in `global.json`, and build output goes to `artifacts/`. Warnings are errors, public members need XML documentation, and the rules in `.editorconfig` (for example `ConfigureAwait(false)` on every await, no `List<T>` in public APIs) fail the build.

## Where code goes

Each API area has its own folder under `src/Zhuwa.Moolre.Sdk/`. Inside it, the client and its enums sit at the folder root; request types go in `Requests/`, result types in `Results/`, and shared data types in `Models/`. One public type per file. The shared core types sit in the project's own `Clients/`, `Configuration/`, `Enums/`, `Requests/`, `Results/` and `Models/` folders; the project root holds no source files. Namespaces follow the API area (`Zhuwa.Moolre.Payments`), not the sub-folders. Anything users should not see goes under `Internal/` and is `internal`. Tests mirror the same folders under `tests/Zhuwa.Moolre.Sdk.Tests/`.

## Public API changes

Every public member must be listed in `src/Zhuwa.Moolre.Sdk/.PublicAPI/PublicAPI.Unshipped.txt`, otherwise the build fails (analyzer RS0016). Your IDE offers a quick fix that adds the line. At release time the maintainers move the lines into `PublicAPI.Shipped.txt`.

`BannedSymbols.txt` lists APIs the library must not call (for example `Console`, `DateTime.Now`, `Enum.IsDefined`, blocking on tasks, `new HttpClient()`); use the suggested alternative instead of suppressing the rule.

## Rules that matter most

- **Money safety:** payments, transfers, sales and messaging calls are never retried automatically. Never generate a new external reference to retry.
- **No secrets:** never put API keys, VAS keys, callback secrets, phone numbers or account numbers in code, tests, issues or logs.
- **Tests:** no network, no credentials. Use the fake `HttpMessageHandler` and the examples in `tests/Zhuwa.Moolre.Sdk.Tests/Fixtures/`. Name tests `Method_Scenario_ExpectedBehaviour`, one Act per test.
- **Breaking changes:** avoid them. Add virtual members and new request properties; mark removals `[Obsolete]` and keep them.

## Pull requests

Describe what changed and why, update `CHANGELOG.md`, and make sure `dotnet build`, `dotnet test` and `dotnet pack` pass with no warnings.
