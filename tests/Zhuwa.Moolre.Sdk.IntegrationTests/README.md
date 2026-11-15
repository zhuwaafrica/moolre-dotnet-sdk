# Integration tests (Moolre sandbox)

These tests call Moolre's real **sandbox** through `AddMoolre`, the same way an application does. They are skipped unless you opt in, so `dotnet test` and CI stay offline. The setup follows Octokit's separate integration test project: credentials come only from environment variables, never from a file.

## Settings

| Variable | Needed for | Example |
| --- | --- | --- |
| `MOOLRE_SANDBOX_TESTS` | every test (the opt-in switch) | `1` |
| `MOOLRE_SANDBOX_API_USER` | every test except the bank list | your sandbox API user |
| `MOOLRE_SANDBOX_ACCOUNT_NUMBER` | every test except the bank list | your sandbox wallet number |
| `MOOLRE_SANDBOX_PHONE` | name check, transfers, collections, Payment ID, bank account | `0241234567` |
| `MOOLRE_SANDBOX_RECEIVER_ACCOUNT_NUMBER` | internal transfer | a second sandbox wallet |
| `MOOLRE_SANDBOX_SMS_VASKEY` | SMS tests | the sandbox SMS service key |
| `MOOLRE_SANDBOX_SMS_SENDERID` | SMS send test | an approved sender ID |
| `MOOLRE_SANDBOX_WHATSAPP_VASKEY` | WhatsApp tests | the sandbox WhatsApp service key (not the SMS key) |
| `MOOLRE_SANDBOX_PRIVATE_KEY`, `MOOLRE_SANDBOX_PUBLIC_KEY` | optional; the sandbox does not check them | |

A test whose settings are missing is skipped, and the skip message names what to set. The wallet test creates its own sandbox wallet, so it needs only `MOOLRE_SANDBOX_API_USER`. The sales tests need a wallet linked to Moolre's "Sell on Our Platform" service.

## Run

```bash
export MOOLRE_SANDBOX_TESTS=1
export MOOLRE_SANDBOX_API_USER=...
export MOOLRE_SANDBOX_ACCOUNT_NUMBER=...
export MOOLRE_SANDBOX_PHONE=...
dotnet test tests/Zhuwa.Moolre.Sdk.IntegrationTests -c Release
```

The sandbox does not move real money. Every call that creates something uses a reference unique to the run (`zhuwa-it-...`), so runs never collide. When the sandbox answers differently from Moolre's documentation, record it and fix the SDK to match the sandbox.
