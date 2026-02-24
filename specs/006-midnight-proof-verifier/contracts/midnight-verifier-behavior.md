# Contract: Midnight Proof System Verifier Behavior

**Feature**: Spec 006 — Midnight Proof System Verifier  
**Date**: 2026-02-24  
**Contract Type**: Behavioral specification  
**Scope**: Deterministic verification behavior for `midnight-zk-v1` proof system

## Contract Overview

This document specifies the behavioral contract for the Midnight proof system verifier implementation. It defines inputs, outputs, deterministic guarantees, failure modes, and performance boundaries.

## Verifier Identity

**Proof System Identifier**: `midnight-zk-v1`  
**Statement Support**: `urn:sigil:statement:license:v1` (initial release)  
**Type**: `IProofSystemVerifier` implementation  
**Registration**: Via `ValidationOptions.AddMidnightZkV1ProofSystem()` in DI configuration

## Input Contract

### Required Inputs

```csharp
Task<ProofVerificationOutcome> VerifyAsync(
    ReadOnlyMemory<byte> proofBytes,
    ProofVerificationContext context,
    CancellationToken cancellationToken = default);
```

**Input Constraints**:
1. `proofBytes`: Raw proof material from envelope; MAY be empty (triggers Invalid outcome)
2. `context`: Non-null verification context containing:
   - `StatementId`: MUST be `urn:sigil:statement:license:v1` (enforced by compatibility gate)
   - Additional context fields as provided by statement handler
3. `cancellationToken`: MUST be respected; verification MUST throw `OperationCanceledException` if cancellation requested

**Pre-Conditions**:
- Verifier instance MUST be registered in proof system registry
- Statement ID in context MUST match verifier compatibility requirements
- Schema validation and registry resolution MUST have succeeded before verifier invocation

## Output Contract

### Success Path

**Outcome**: `ProofVerificationOutcome.Verified()`

**Conditions**:
- Cryptographic verification succeeded for given `proofBytes` and `context`
- No internal verifier faults occurred
- Verification completed deterministically

**Post-Conditions**:
- Validation pipeline proceeds to post-crypto stages (expiry evaluation per Spec 002)
- No failure code is set (verified state)

### Cryptographic Failure Path

**Outcome**: `ProofVerificationOutcome.Invalid(LicenseFailureCode.ProofVerificationFailed)`

**Conditions**:
- `proofBytes` are cryptographically invalid for given context
- Proof does not satisfy zero-knowledge verification constraints
- Proof is well-formed but does not verify

**Maps To**: `LicenseStatus.Invalid` (Spec 002)

**Post-Conditions**:
- Validation pipeline terminates with Invalid status
- MUST NOT map to `Expired` or `Unsupported` (FR-006)

### Context Incompatibility Path

**Outcome**: `ProofVerificationOutcome.Invalid(LicenseFailureCode.ProofVerificationContextIncompatible)`

**Conditions**:
- `context.StatementId` is not supported by Midnight verifier
- Initial release: any statement ID other than `urn:sigil:statement:license:v1`

**Maps To**: `LicenseStatus.Invalid` (Spec 002)

**Post-Conditions**:
- Validation pipeline terminates with Invalid status
- Clear diagnostic message indicating statement incompatibility (no sensitive data)

### Internal Fault Path

**Outcome**: `ProofVerificationOutcome.Error(LicenseFailureCode.ProofVerifierInternalError)`

**Conditions**:
- Unexpected exception during verification
- Verifier initialization failure
- Corrupted internal verifier state
- Any non-validation fault within verifier

**Maps To**: `LicenseStatus.Error` (Spec 002)

**Post-Conditions**:
- Validation pipeline terminates with Error status
- Exception details MUST NOT include `proofBytes` (FR-011)
- Internal diagnostics MAY log redacted error context (opt-in, FR-012)

## Determinism Guarantees

### Identical Input Determinism

**Rule**: For identical inputs (`proofBytes`, `context`, verifier configuration), verification MUST produce identical outcomes across repeated invocations.

**Test**: SC-001 conformance vectors executed 100 times MUST yield identical results.

**Enforcement**:
- No randomness in verification logic
- No time-dependent behavior (time is injected via clock abstraction elsewhere)
- No network calls (FR-004)
- No mutable global state

### Configuration Determinism

**Rule**: For fixed SDK version and fixed DI configuration, verifier behavior MUST be stable across process restarts.

**Enforcement**:
- Initialization MUST be deterministic (FR-009)
- No environment-dependent verification paths
- Cryptographic parameters immutable after initialization

## Failure Mode Mapping

| Verifier Condition | Outcome | Failure Code | Maps To Status | SC Coverage |
|--------------------|---------|--------------|----------------|-------------|
| Crypto verification succeeds | Verified | N/A | (continues to expiry check) | SC-001 |
| Crypto verification fails | Invalid | ProofVerificationFailed | Invalid | SC-002 |
| Statement context incompatible | Invalid | ProofVerificationContextIncompatible | Invalid | SC-002 |
| Internal verifier exception | Error | ProofVerifierInternalError | Error | SC-003 |
| Empty proofBytes | Invalid | ProofVerificationFailed | Invalid | SC-002 |
| Malformed proofBytes | Invalid | ProofVerificationFailed | Invalid | SC-002 |

## Performance Contract

### Latency Bounds

**Rule**: Midnight verification MUST consume ≤ 60% of end-to-end validation p95 budget.

**Baseline** (from Spec 002 SC-004):
- End-to-end p95: < 1 second for envelopes ≤ 10KB, offline
- **Midnight budget**: ≤ 600ms p95

**Measurement**:
- Use Spec 002 SC-004 workload profile (fixed corpus, warmed process, ≥100 samples)
- Measure wall-clock time for `VerifyAsync` call within validation pipeline
- Assert p95 ≤ 600ms in `ValidationPerformanceBenchmarks.cs`

### Initialization Performance

**Rule**: Initialization (if required) MUST be deterministic and complete within reasonable startup time.

**Enforcement**:
- Lazy initialization defers cost to first verification
- Initialization failure MUST produce deterministic Error outcome
- Subsequent verifications MUST reuse initialized state (no re-initialization)

## Thread Safety Contract

**Rule**: Verifier instance MUST be safe for concurrent invocations of `VerifyAsync` across multiple threads.

**Enforcement** (FR-010):
- Internal state MUST be immutable after initialization
- Verification outcomes MUST remain deterministic under concurrent load
- No shared mutable state between verification requests

**Test**: SC-001 conformance executed with concurrent validations MUST yield identical per-vector outcomes.

## Security & Confidentiality Contract

### Proof Material Confidentiality (FR-011)

**Rule**: `proofBytes` MUST NEVER be logged, serialized, or emitted in diagnostics.

**Enforcement**:
- No `proofBytes` in exception messages
- No `proofBytes` in structured logs (any log level)
- Diagnostics MUST be opt-in and MUST redact proof material (FR-012)

**Test**: SC-005 logs and exception messages inspected for zero `proofBytes` leakage.

### Error Message Safety (FR-013)

**Rule**: Public-facing error messages MUST NOT expose sensitive verifier internals.

**Allowed**:
- Failure code (e.g., `ProofVerificationFailed`)
- Statement compatibility status
- Generic "verification failed" message

**Forbidden**:
- Cryptographic intermediate values
- Internal verifier state details
- `proofBytes` content or structure

## Conformance Requirements

### Minimum Vector Coverage (FR-015)

Conformance suite MUST include:
1. **Known-valid vector** for `urn:sigil:statement:license:v1`
   - Expected outcome: `Verified`
   - Maps to: `LicenseStatus.Valid` (after expiry check passes)

2. **Known-invalid vector** (crypto failure)
   - Expected outcome: `Invalid(ProofVerificationFailed)`
   - Maps to: `LicenseStatus.Invalid`

3. **Simulated internal-error vector**
   - Expected outcome: `Error(ProofVerifierInternalError)`
   - Maps to: `LicenseStatus.Error`

### Conformance Test Execution

**Assertions**:
- Each vector MUST produce expected outcome (SC-001: 100% determinism)
- Invalid vectors MUST map to `Invalid` status (SC-002: 0 `Expired`/`Unsupported` mappings)
- Error vectors MUST map to `Error` status (SC-003: 100% Error mapping)

## Integration Points

### Validation Pipeline Integration

**Entry Point**: `LicenseValidator` invokes `VerifyAsync` during proof verification stage (after schema, before expiry).

**Exit Points**:
- Verified → proceed to expiry evaluation
- Invalid → terminate with Invalid status and failure code
- Error → terminate with Error status and failure code

### DI Registration Integration

**Registration**:
```csharp
services.AddSigilValidation(options =>
{
    options.AddMidnightZkV1ProofSystem();
});
```

**Effect**:
- Midnight verifier registered in `ImmutableProofSystemRegistry` with key `midnight-zk-v1`
- Verifier lifecycle: Singleton (one instance per process)
- Thread-safe for concurrent use

## Scope Boundaries

**In Scope**:
- Cryptographic verification for `midnight-zk-v1` proof system
- Deterministic verification for `urn:sigil:statement:license:v1` context
- Failure mapping to `Invalid` and `Error` statuses

**Out of Scope** (FR-017, FR-018):
- Statement claim interpretation (handled by statement handler)
- Additional statement ID support beyond `license:v1` (future extension)
- Proof issuance, wallet management, platform services (verification-only SDK)

## Versioning & Compatibility

**Statement ID Support**:
- Initial release: `urn:sigil:statement:license:v1` only
- Future releases MAY add support for additional statement IDs without breaking changes
- Unsupported statement IDs MUST return `ProofVerificationContextIncompatible` (deterministic rejection)

**Proof System Identifier**:
- `midnight-zk-v1` is immutable across SDK versions (breaking change if modified)
- Additional Midnight versions (e.g., `midnight-zk-v2`) would be separate verifier implementations with distinct identifiers
