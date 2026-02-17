# Data Model — SDK Validation API (Spec 002)

**Spec**: [specs/002-sdk-validation-api/spec.md](spec.md)  
**Depends on**: Spec 001 Proof Envelope Format

## Primary Entities

### Proof Envelope (Spec 001)

A versioned JSON document with required top-level fields:
- `envelopeVersion`
- `proofSystem`
- `statementId`
- `proofBytes`
- `publicInputs`

`publicInputs` contains a fixed schema for Spec 001 v1.0 including optional `expiresAt`.

### LicenseStatus

A coarse-grained outcome used for application branching:
- `Valid`: Envelope is supported, proof verified, and semantic checks pass.
- `Invalid`: Envelope is well-formed and supported, but verification/semantic checks fail.
- `Expired`: Envelope is supported and verified, but is expired at evaluation time.
- `Unsupported`: Envelope is well-formed enough to identify but is not supported (unknown `envelopeVersion`, `proofSystem`, or `statementId`).
- `Malformed`: Input cannot be parsed or does not satisfy Spec 001 schema/required structure.
- `Error`: Unexpected internal failure (IO, exceptions, initialization errors).

### Failure Code

A stable, deterministic identifier representing a single failure reason.

Properties:
- Stable across minor versions (never reused).
- Maps deterministically to exactly one `LicenseStatus`.
- Returned based on a strict, ordered validation pipeline (first failing stage wins).

### Validation Result

A structured result object that includes:
- `Status`: the `LicenseStatus`.
- Extracted identifiers when available (`EnvelopeVersion`, `StatementId`, `ProofSystem`).
- Optional typed claims when `Status == Valid`.
- Optional `Failure` object (containing failure code and safe message).

### Immutable Registries

Registries are application-provided, DI-constructed catalogs used to resolve:
- Proof system verifier for `proofSystem`.
- Statement handler for `statementId`.

Registry requirements:
- Immutable after construction.
- No runtime mutation.
- Resolution is deterministic.

## Validation State Machine (conceptual)

- **Input** → Parse JSON → Extract identifiers (best-effort)
- Schema validation (Spec 001) gate
- Registry resolution gate
- Proof verification
- Semantic checks (including expiry)
- Result synthesis

At any gate failure: return non-`Valid` result (fail closed).
