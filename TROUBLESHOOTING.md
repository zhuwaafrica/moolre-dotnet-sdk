# Troubleshooting

Based on Moolre's documented codes. The SDK is a preview with no endpoints implemented yet, so this describes what each situation will mean once calls exist.

## Authentication errors (`AIN01`, `AIN03`, `AIN04`, `IN01`, `ASL00`, `ASS00`)

- Use the credentials of the **same wallet's** API Service; a key for one wallet cannot act on another.
- Transfers and sales need the **private** key (`X-API-KEY`); collections, payment links and Payment IDs need the **public** key (`X-API-PUBKEY`); SMS and WhatsApp each need their own `X-API-VASKEY`.
- `AIN04` means API access is not activated for the wallet.
- Sandbox and live credentials differ; check `MoolreOptions.Environment`.

## Duplicate reference (`TP13`, `TR013`, `APY18`, `ASL08`, `INP02`)

The external reference must be unique. After a timeout, do **not** generate a new reference: look the operation up by its original reference.

## A timeout, or a sale code `ASL06`

The operation may have gone through. The SDK throws `MoolreOutcomeUncertainException` with the external reference. Check the status (Payment Status, Transfer Status, Sales Status) before doing anything else. `ASL11` means "in progress", not "delivered".

## OTP required (`TP14`)

Not a failure: the payer must confirm a code sent by SMS. Send the request again with the code.

## Disbursements fail after setup

Transfers can be restricted to whitelisted server IP addresses per API Service in the Moolre app. Confirm your server's public IP is on that list.

## Webhooks never arrive or fail verification

- Allow Moolre's six callback source IP addresses: IPv4 `174.138.44.22`, `174.138.40.67`, `192.241.135.134`; IPv6 `2604:a880:400:d1:0:3:4cf0:c001`, `2604:a880:400:d0::1a77:4001`, `2604:a880:400:d0::4234:2001` (from Moolre's documentation, which can change).
- Compare the payload `secret` with the secret for your wallet. Match on `externalref`, handle duplicate deliveries, and confirm high-value orders with the status call.

## Still stuck

Remove all secrets and personal data, then open an issue with the code, the application code returned, and the SDK version. Problems with Moolre's own service go to Moolre support.
