# Data Model: Midnight Proof System Verifier

**Feature**: Spec 006 — Midnight Proof System Verifier  
**Date**: 2026-02-24  
**Context**: Phase 1 design — entities, relationships, and lifecycle

## Core Entities

### Proof System Identifier

**Entity**: `ProofSystemIds.MidnightZkV1`  
**Type**: Constant string  
**Value**: `"midnight-zk-v1"`  
**Lifecycle**: Compile-time constant; immutable  
**Relationships**:
- Referenced by DI registration in `ValidationOptions.AddMidnightZkV1ProofSystem()`
- Matched against `proofSystem` field in proof envelopes during validation
- Used as registry key in `ImmutableProofSystemRegistry`

**Validation Rules**:
- MUST be exactly `"midnight-zk-v1"` (case-sensitive, ordinal comparison per Spec 005)
- MUST NOT change across SDK versions (breaking change)

---

### Midnight Verifier Instance

**Entity**: `MidnightZkV1ProofSystemVerifier`  
**Type**: Implementation of `IProofSystemVerifier` (Spec 005 contract)  
**Lifecycle**: Singleton registered via DI; constructed once per process  
**State**:
- Cryptographic verification context (lazily initialized)
- Immutable after initialization
- Thread-safe for concurrent reuse

**Relationships**:
- Implements `IProofSystemVerifier` contract
- Registered in `ImmutableProofSystemRegistry` with key `ProofSystemIds.MidnightZkV1`
- Invoked by `LicenseValidator` during proof verification stage
- Returns `ProofVerificationOutcome` to validation pipeline

**Validation Rules**:
- MUST operate deterministically for identical inputs
- MUST NOT mutate state during verification
- MUST NOT perform network access (FR-004)
- MUST support cancellation via `CancellationToken`

---

### Proof Verification Context

**Entity**: `ProofVerificationContext` (Spec 005 contract; reused)  
**Type**: Value object containing statement-scoped verification input  
**Lifecycle**: Created per validation request; immutable  
**Fields**:
- `StatementId` (string): Statement identifier (e.g., `"urn:sigil:statement:license:v1"`)
- Additional context fields as defined by statement handler

**Relationships**:
- Constructed by `LicenseValidator` from statement handler output
- Passed to Midnight verifier's `VerifyAsync` method
- Scoped to current validation request; no cross-request state

**Validation Rules**:
- `StatementId` MUST match Midnight-supported statement ID (FR-016: `license:v1` initially)
- Context MUST NOT contain sensitive proof material for logging safety

---

### Proof Verification Outcome

**Entity**: `ProofVerificationOutcome` (Spec 005 contract; reused)  
**Type**: Discriminated result representing verification success or failure  
**Lifecycle**: Returned by verifier; consumed by validation pipeline  
**States**:
- **Verified**: Cryptographic proof is valid for given context
- **Invalid**: Proof verification failed (maps to `LicenseStatus.Invalid`)
- **Error**: Internal verifier fault (maps to `LicenseStatus.Error`)

**Relationships**:
- Returned by `MidnightZkV1ProofSystemVerifier.VerifyAsync`
- Mapped to `LicenseFailureCode` by `LicenseValidator`
- Determines pipeline continuation (verified → expiry check; invalid/error → fail)

**Validation Rules**:
- MUST be deterministic for identical inputs and configuration
- MUST NOT expose `proofBytes` in diagnostic fields (FR-011)
- MUST include failure code when state is Invalid or Error

---

### Conformance Vector

**Entity**: Conformance test vector (JSON artifact)  
**Type**: Test data structure  
**Lifecycle**: Loaded from `tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/*.json` during test execution  
**Fields**:
```json
{
  "vectorId": "license-v1-valid",
  "description": "Known-valid Midnight proof for license:v1",
  "proofSystem": "midnight-zk-v1",
  "statementId": "urn:sigil:statement:license:v1",
  "envelopeJson": "{ ... }",
  "expectedOutcome": "Verified",
  "expectedFailureCode": null
}
```

**Relationships**:
- Loaded by `MidnightConformanceVectors` utility
- Consumed by `MidnightProofConformanceTests`
- Maps expected outcomes to `ProofVerificationOutcome` assertions

**Validation Rules** (FR-015):
- MUST include at least one known-valid vector for `license:v1`
- MUST include at least one known-invalid vector
- MUST include at least one simulated internal-error vector

---

## Entity Relationships Diagram

```text
┌─────────────────────────────────┐
│   Proof Envelope (Spec 001)     │
│   { proofSystem, proofBytes }   │
└────────────┬────────────────────┘
             │
             │ routed by proofSystem="midnight-zk-v1"
             ↓
┌─────────────────────────────────┐
│ ImmutableProofSystemRegistry    │
│ (Spec 005)                      │
│                                 │
│ [midnight-zk-v1] ────────────┐  │
└─────────────────────────────┼───┘
                              │
                              ↓
              ┌───────────────────────────────────┐
              │ MidnightZkV1ProofSystemVerifier   │
              │ (Spec 006)                        │
              │                                   │
              │  VerifyAsync(proofBytes, context) │
              └────────────┬──────────────────────┘
                           │
                           │ uses
                           ↓
              ┌───────────────────────────────┐
              │ ProofVerificationContext      │
              │ (Spec 005)                    │
              │  - StatementId: license:v1    │
              └───────────────────────────────┘
                           │
                           │ returns
                           ↓
              ┌───────────────────────────────┐
              │ ProofVerificationOutcome      │
              │ (Spec 005)                    │
              │  - Verified / Invalid / Error │
              └────────────┬──────────────────┘
                           │
                           │ mapped to
                           ↓
              ┌───────────────────────────────┐
              │ LicenseFailureCode            │
              │ (Spec 002)                    │
              │  - ProofVerificationFailed    │
              │  - ProofVerifierInternalError │
              └───────────────────────────────┘
```

## State Transitions

### Verifier Initialization Lifecycle

```text
[Uninitialized] 
     │
     │ First VerifyAsync() call
     ↓
[Initializing] ──────→ [Initialization Failed] ──→ [Error Outcome]
     │                        ↑
     │                        │ exception during init
     ↓                        │
[Initialized] ────────────────┘
     │
     │ Subsequent VerifyAsync() calls
     ↓
[Verification] ──→ [Verified] or [Invalid] or [Error]
     │
     │ (state remains Initialized; thread-safe reuse)
     ↓
[Initialized]
```

### Verification Outcome Transitions

```text
proofBytes + context
     │
     ├──→ [Crypto verification succeeds] ──→ Verified
     │
     ├──→ [Crypto verification fails] ──→ Invalid (ProofVerificationFailed)
     │
     ├──→ [Statement context incompatible] ──→ Invalid (ProofVerificationContextIncompatible)
     │
     └──→ [Internal verifier fault] ──→ Error (ProofVerifierInternalError)
```

## Validation Rules Summary

| Entity | Key Rules |
|--------|-----------|
| ProofSystemIds.MidnightZkV1 | Immutable constant; ordinal comparison |
| MidnightZkV1ProofSystemVerifier | Deterministic; offline; thread-safe; no proofBytes logging |
| ProofVerificationContext | Immutable; statement-scoped; no sensitive material |
| ProofVerificationOutcome | Deterministic; failure code required for Invalid/Error states |
| ConformanceVector | Minimum 3 vectors (valid, invalid, internal-error) per FR-015 |

## Performance Considerations

- Lazy initialization defers cryptographic context setup until first verification
- Thread-safe state reuse avoids repeated initialization overhead
- Midnight verification time MUST be ≤ 60% of end-to-end p95 budget (SC-004)
- Conformance vectors kept minimal (<10KB envelopes) to align with SC-004 workload profile
