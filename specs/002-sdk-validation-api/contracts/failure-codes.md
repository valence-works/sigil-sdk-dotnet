# Contract — Failure Codes (Spec 002)

Failure codes are deterministic identifiers returned in the validation result when `Status != Valid`.

## Rules

- Each failure code MUST map deterministically to exactly one `LicenseStatus`.
- Failure codes MUST be stable across minor versions (never reused; meaning never changes).
- Validation MUST return the code for the first failing stage in the validation pipeline.

## Suggested Initial Set (v1)

| Failure Code | Status | Meaning |
|---|---|---|
| `InvalidJson` | `Malformed` | Input could not be parsed as JSON. |
| `SchemaValidationFailed` | `Malformed` | JSON parsed but did not conform to Spec 001 schema. |
| `UnsupportedEnvelopeVersion` | `Unsupported` | `envelopeVersion` is syntactically valid but not supported. |
| `UnsupportedProofSystem` | `Unsupported` | `proofSystem` is syntactically valid but not registered/supported. |
| `UnsupportedStatement` | `Unsupported` | `statementId` is syntactically valid but not registered/supported. |
| `ProofBytesInvalid` | `Malformed` | `proofBytes` could not be decoded as base64 (defense-in-depth beyond schema). |
| `ProofVerificationFailed` | `Invalid` | Cryptographic verification failed for a supported statement/proof system. |
| `StatementValidationFailed` | `Invalid` | Statement semantic validation failed for a supported statement. |
| `LicenseExpired` | `Expired` | Verified envelope is expired at evaluation time. |
| `ExpiresAtInvalid` | `Malformed` | `expiresAt` was present but not a valid RFC 3339/ISO-8601 date-time. |
| `StreamReadFailed` | `Error` | Failed to read the input stream (IO/disposal during read). |
| `InternalError` | `Error` | Unexpected internal failure. |

Notes:
- Failure codes are implemented as a public enum with explicit numeric values in `src/Sigil.Sdk/Validation/LicenseFailureCode.cs`.
- `ExpiresAtInvalid` is defense-in-depth; with schema-first validation it should be rare in practice.

## Status Classification Guidance

- Use `Malformed` for parse/schema/type/required-field failures.
- Use `Unsupported` when the envelope is understood but capability is missing.
- Use `Invalid` for verification/semantic failures after schema + registry resolution.
- Use `Expired` only when expiry can be concluded from verified claims.
- Use `Error` for unexpected failures (IO, exceptions, initialization problems).
