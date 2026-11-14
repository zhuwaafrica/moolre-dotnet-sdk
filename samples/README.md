# Samples

Planned sample applications. They are added together with the code they exercise and are built (and, where possible, run against the sandbox) in CI so they stay working.

| Sample | Shows |
| --- | --- |
| `PaymentCollection` | Register with `AddMoolre`, request a mobile money payment, check its status, handle OTP and pending states |
| `WebhookReceiver` | A minimal ASP.NET Core endpoint: verify the secret, match `externalref`, handle duplicate delivery, confirm before fulfilling |
| `Transfers` | Validate a recipient, send a transfer, and reconcile after a timeout instead of resending |
| `Sms` | Send an SMS and check its delivery status |

No samples exist yet.
