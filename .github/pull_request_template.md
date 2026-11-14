## What changed and why

<!-- Describe the change. Link the issue if there is one. -->

## Checklist

- [ ] `dotnet build -c Release`, `dotnet test -c Release` and `dotnet pack src/Zhuwa.Moolre.Sdk -c Release` pass with no warnings
- [ ] Public API changes are listed in `src/Zhuwa.Moolre.Sdk/.PublicAPI/PublicAPI.Unshipped.txt`
- [ ] Tests added or updated (no network, no credentials; use `tests/Zhuwa.Moolre.Sdk.Tests/Fixtures/`)
- [ ] `CHANGELOG.md` updated
- [ ] No secrets, phone numbers or account numbers anywhere in the change
- [ ] No automatic retry added for payments, transfers, sales or messaging
