# Data Model — Proof System Verifier Contract (Spec 005)

**Spec**: [specs/005-proof-system-verifier-contract/spec.md](spec.md)  
**Depends on**: Spec 001 Proof Envelope Format, Spec 002 SDK Validation API, Spec 003 DI Integration, Spec 004 Statement Handler Contract

## Primary Entities

### ProofSystemVerifier

Represents a verifier implementation bound to one canonical `proofSystem` identifier.

Core fields/attributes:
- `proofSystemId` (required, canonical, case-sensitive)
- `supportsOfflineVerification` (`true` required)
- `version` (optional metadata)

Behavioral invariants:
- Deterministic for identical inputs and registry configuration.
- Does not infer or reinterpret statement semantics.
- Never emits `proofBytes` in logs, exceptions, or diagnostics.

### VerificationContext

Statement-scoped context produced by the selected statement handler and consumed by the verifier.

Core fields/attributes:
- `statementId` (required)
- `contextPayload` (required, statement-specific)
- `contextVersion` (optional)

Behavioral invariants:
- Treated as opaque by validator orchestration except for routing and diagnostics metadata.
- Context incompatibility with proof material maps to `Invalid` for supported proof systems.

### ProofVerifierRegistry

Immutable runtime mapping from canonical `proofSystem` identifier to exactly one `ProofSystemVerifier`.

Core fields/attributes:
- `entries` (`Dictionary<string, ProofSystemVerifier>` conceptually)
- `isImmutable` (`true` after DI construction)

Behavioral invariants:
- Duplicate keys are rejected before runtime verification.
- Unknown key lookups fail deterministically as `Unsupported`.

### VerificationOutcome

Deterministic verifier result consumed by the validation pipeline.

States:
- `Verified`
- `InvalidProof` (includes proof/context incompatibility)
- `VerifierError`
- `Cancelled` (control flow, not mapped to validation status)

Pipeline mapping:
- `InvalidProof` -> `LicenseStatus.Invalid`
- `VerifierError` -> `LicenseStatus.Error`
- `Cancelled` -> propagated cancellation (no status mapping)

### ValidationStageResult

Represents first-failing-stage decision in pipeline orchestration.

Core fields/attributes:
- `stageName` (schema, routing, proof, statement, expiry)
- `status` (`Valid`, `Invalid`, `Expired`, `Unsupported`, `Malformed`, `Error`)
- `failureCode` (deterministic, exactly one when status is non-valid)

Behavioral invariants:
- Earlier stage failures are never overridden by later stages.
- `proofBytes` redaction policy applies to all stage diagnostics.

## Relationships

- One `ProofVerifierRegistry` contains many `ProofSystemVerifier` entries, keyed by canonical `proofSystemId`.
- One envelope resolves to one verifier via `proofSystemId` lookup.
- One statement handler produces one `VerificationContext` per validation execution.
- One verifier consumes one `VerificationContext` + `proofBytes` and yields one `VerificationOutcome`.
- One `VerificationOutcome` is transformed by pipeline rules into one `ValidationStageResult` (except cancellation, which bypasses status mapping).

## Validation State Transitions

1. Envelope passes schema and identifier extraction stages (Spec 001 / Spec 002).
2. Registry resolves canonical `proofSystemId`.
3. Statement handler supplies statement-specific `VerificationContext`.
4. Verifier executes with `proofBytes` + context offline.
5. Outcome mapping:
   - `Verified` -> continue to next pipeline stages (statement/expiry as applicable).
   - `InvalidProof` -> stop with `Invalid` (first failing stage wins).
   - `VerifierError` -> stop with `Error`.
   - `Cancelled` -> propagate cancellation to caller (no validation status returned).
6. Diagnostics emitted only under configured policy and always with proof material redacted.
