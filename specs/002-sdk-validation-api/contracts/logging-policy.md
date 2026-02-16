# Contract — Structured Logging & Diagnostics Policy (Spec 002)

## Non-Negotiable Rules

- The SDK MUST NOT log `proofBytes` at any log level under any configuration.
- The SDK MUST NOT log the raw envelope JSON.
- The SDK MUST NOT emit `proofBytes` (or any derivative such as decoded bytes, hashes/fingerprints) in diagnostics.
- Diagnostics MUST be opt-in and disabled by default.
- Logging MUST be structured (stable templates + named properties).
- Logging MUST NOT change validation outcomes (failures to log must not throw).

## Allowed Structured Properties

When available and safe, logs MAY include:
- `LicenseStatus`
- `FailureCode`
- `EnvelopeVersion`
- `StatementId`
- `ProofSystem`
- `ValidationStage`

## Diagnostics Mode

When diagnostics are enabled explicitly, logs MAY include:
- Error counts and locations (e.g., schema error count; JSON Pointer paths)
- Exception type names

Diagnostics MUST NOT include:
- Any payload values
- Any portion of `proofBytes`
- Any raw JSON
