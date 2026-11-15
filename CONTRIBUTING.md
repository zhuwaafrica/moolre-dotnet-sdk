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

The library copies the folder layout of the official Square .NET SDK. Every file has exactly one home:

| Folder (under `src/Zhuwa.Moolre.Sdk/`) | What goes there | Namespace |
| --- | --- | --- |
| project root | `MoolreClient` and `MoolreClientFactory` only | `Zhuwa.Moolre` |
| `<Area>/` (`Accounts`, `Transfers`, `Payments`, ...) | the area client `Moolre<Area>`, its internal `DefaultMoolre<Area>` and `<Area>Mapper` | `Zhuwa.Moolre.<Area>` |
| `<Area>/Requests/` | the `...Request` types for that area's calls | `Zhuwa.Moolre.<Area>` |
| `Types/<Area>/` | that area's results, models and enums (grouped per area, as Stripe groups `Entities/`) | `Zhuwa.Moolre` |
| `Types/_common/` | types every area shares: `MoolreResult`, response codes, `MoolreTransaction`, `Currency`, ... | `Zhuwa.Moolre` |
| `Core/` | internal plumbing: transport, JSON reading, validation | `Zhuwa.Moolre.Core` |
| `Core/Public/` | options, environment, exceptions | `Zhuwa.Moolre` |
| `DependencyInjection/` | `AddMoolre` | `Zhuwa.Moolre.DependencyInjection` |

One type per file, named after the type. Tests mirror the same folders under `tests/Zhuwa.Moolre.Sdk.Tests/`. `FolderConventionTests` checks these rules on every build, so a file in the wrong place fails the tests.

## Sandbox (integration) tests

`tests/Zhuwa.Moolre.Sdk.IntegrationTests/` calls Moolre's real sandbox. Its tests are skipped unless `MOOLRE_SANDBOX_TESTS=1` and the credentials they need are set as environment variables; see that folder's README. Never write credentials in a file. Every new operation gets a sandbox test there alongside its unit tests.

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
