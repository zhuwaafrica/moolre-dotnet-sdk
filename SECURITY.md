# Security policy

## Reporting a vulnerability

Please **do not** open a public issue for a security problem. Report it privately through GitHub's "Report a vulnerability" feature on this repository (Security tab), or email the maintainers at Zhuwa Africa.

Include what you found, how to reproduce it, and which version is affected. We aim to acknowledge reports within a few working days.

## Scope

This library sends credentials and payment instructions to Moolre. Relevant reports include credential leakage (logs, exceptions, redirects), payment instructions sent twice, and unsafe handling of webhook payloads.

## Never share secrets

Do not include real API keys, VAS keys, callback secrets, phone numbers or account numbers in issues, pull requests or logs. Use the sandbox and placeholder values.

Problems in Moolre's own API or app should be reported to Moolre, not here.
