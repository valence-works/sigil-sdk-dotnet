# Feature Specification: Midnight Proof System Verifier

**Feature Branch**: `001-midnight-proof-verifier`  
**Created**: 2026-02-24  
**Status**: Draft  
**Input**: User description: "Create Spec 006: Midnight Proof System Verifier for the Sigil SDK"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify Midnight proofs in validation pipeline (Priority: P1)

As an SDK consumer, I need envelopes with `proofSystem` set to the Midnight identifier to be verified deterministically and offline so I can trust validation outcomes in production and test environments.

**Why this priority**: This is the core feature value: cryptographic proof validity for the first supported proof system.

**Independent Test**: Can be fully tested by submitting envelopes using the Midnight proof system with known-valid and known-invalid vectors and confirming deterministic `Verified` vs `Not Verified` behavior and failure mapping.

**Acceptance Scenarios**:

1. **Given** a valid envelope using the canonical Midnight `proofSystem` identifier and a matching statement verification context, **When** validation runs after schema checks, **Then** the Midnight verifier returns `Verified` and the pipeline continues to post-crypto checks.
2. **Given** an envelope using the canonical Midnight `proofSystem` identifier with invalid `proofBytes`, **When** validation runs, **Then** the verifier returns deterministic `Not Verified` and the pipeline returns an `Invalid`-mapped failure.
3. **Given** identical input envelope, statement verification context, and verifier configuration, **When** validation is run repeatedly, **Then** the verification outcome is identical for every run.
4. **Given** a validation request for Midnight verification, **When** verification executes, **Then** no network access is required or attempted.

---

### User Story 2 - Diagnose failures safely without leaking proof material (Priority: P2)

As an SDK operator, I need clear, deterministic failure categories for invalid proofs vs internal verifier faults, with safe diagnostics that never reveal proof material.

**Why this priority**: Correct failure semantics and secure diagnostics are required for fail-closed behavior and operational safety.

**Independent Test**: Can be independently tested by forcing invalid-proof and internal-error paths and asserting status mapping plus redaction and message safety rules.

**Acceptance Scenarios**:

1. **Given** a cryptographically invalid Midnight proof, **When** verification fails, **Then** the failure maps to `Invalid` and never to `Expired` or `Unsupported`.
2. **Given** an internal verifier fault (for example, unexpected exception or corrupted verifier state), **When** verification cannot complete, **Then** the failure maps to `Error` deterministically.
3. **Given** diagnostics are disabled, **When** verification fails, **Then** no raw proof material is logged or emitted.
4. **Given** diagnostics are enabled, **When** verification fails, **Then** diagnostics remain redacted and do not include `proofBytes` or sensitive verifier internals.

---

### User Story 3 - Reuse initialized Midnight verifier state safely (Priority: P3)

As an SDK integrator, I need one-time verifier initialization and safe reuse across validations so cryptographic verification stays within validation performance targets.

**Why this priority**: Initialization and reuse strongly influence throughput and latency while preserving deterministic behavior.

**Independent Test**: Can be independently tested by validating multiple envelopes over a single process lifetime and confirming deterministic outcomes, startup readiness, and stable performance.

**Acceptance Scenarios**:

1. **Given** the SDK starts with Midnight verifier registration, **When** initialization completes, **Then** verifier state is ready for deterministic verification without runtime mutation requirements.
2. **Given** repeated validations in one process lifetime, **When** the same initialized verifier state is reused safely, **Then** verification outcomes remain deterministic and fail-closed.
3. **Given** envelopes of up to 10KB under representative load, **When** validation runs, **Then** Midnight verification does not dominate the end-to-end validation budget defined by Spec 002.

### Edge Cases

- Midnight verifier is registered, but statement context is missing or inconsistent with `statementId`; verifier returns deterministic failure mapped to `Invalid`.
- `proofBytes` are empty, truncated, or malformed binary; verifier treats as invalid proof input and returns deterministic `Invalid` mapping.
- Internal verifier initialization succeeds at startup but runtime state becomes unusable; verifier fails closed with deterministic `Error` mapping.
- Multiple validations occur concurrently with shared initialized state; outcomes remain deterministic and isolated per request.
- Unknown `proofSystem` values are handled before this verifier is selected (per Spec 002 registry behavior) and are out of scope for this verifier implementation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST define and use a single canonical proof system identifier for Midnight verification: `midnight-zk-v1`.
- **FR-002**: The Midnight verifier MUST be registerable through SDK dependency injection so that envelopes declaring `proofSystem = "midnight-zk-v1"` resolve to this verifier.
- **FR-003**: For any given `proofBytes` and statement-provided verification context, the Midnight verifier MUST return a deterministic verification outcome (`Verified` or `Not Verified`).
- **FR-004**: The Midnight verifier MUST operate fully offline and MUST NOT require or perform network access during verification.
- **FR-005**: When cryptographic proof verification fails, the verifier integration MUST produce a deterministic failure that maps to `Invalid` status per Spec 002.
- **FR-006**: Cryptographic verification failure for Midnight MUST NOT map to `Expired` or `Unsupported` statuses.
- **FR-007**: When the verifier encounters internal faults (including unexpected exceptions or corrupted verifier state), the verifier integration MUST produce a deterministic failure that maps to `Error` status per Spec 002.
- **FR-008**: The verifier MUST verify only cryptographic validity for the provided statement context and MUST NOT interpret statement claims or business semantics.
- **FR-009**: The verifier MAY perform one-time initialization at startup; if initialization is required, it MUST be deterministic and safe for repeated process starts with the same configuration.
- **FR-010**: Initialized verifier state MUST be safe to reuse across multiple validation operations without changing verification outcomes for identical inputs.
- **FR-011**: The verifier and its integration MUST never log, serialize, or emit raw `proofBytes` at any log level or in exception payloads.
- **FR-012**: Detailed diagnostics for Midnight verification failures MUST be opt-in and MUST redact proof material and sensitive verifier internals.
- **FR-013**: Public-facing error messages originating from Midnight verification MUST avoid exposing sensitive internal verifier details while still allowing deterministic failure classification.
- **FR-014**: The test harness MUST support a conformance-suite structure for Midnight verification vectors, including known-good and known-bad cases.
- **FR-015**: Conformance coverage MUST include, at minimum, one known-valid vector for `urn:sigil:statement:license:v1`, one known-invalid vector, and one simulated internal-error vector that verifies `Error` mapping.
- **FR-016**: The Midnight verifier MUST support `urn:sigil:statement:license:v1` statement verification context at initial release.
- **FR-017**: Support for additional statement IDs MAY be added later, but this feature MUST NOT define semantics or requirements for additional statements.
- **FR-018**: This feature MUST remain within verification-only SDK scope and MUST NOT introduce issuance, wallet management, or platform service responsibilities.

### Key Entities *(include if feature involves data)*

- **Proof System Verifier Registration**: Registry entry that binds canonical identifier `midnight-zk-v1` to the Midnight verifier in DI configuration.
- **Midnight Verification Context**: Statement-provided cryptographic input context used by the verifier alongside `proofBytes`; scoped to the current `statementId`.
- **Midnight Verification Result**: Deterministic verifier output indicating `Verified` or `Not Verified`, plus deterministic failure classification when applicable.
- **Conformance Vector**: Test artifact containing statement ID, proof system identifier, proof input payload reference, expected outcome, and expected failure mapping.

## Assumptions

- Validation pipeline ordering from Spec 002 remains unchanged: schema validation occurs before cryptographic verification, and expiry evaluation occurs after cryptographic verification with injected clock semantics.
- Unknown or unregistered `proofSystem` handling remains owned by validator registry enforcement from Spec 002 and Spec 005, not by the Midnight verifier implementation.
- The canonical identifier `midnight-zk-v1` is available to SDK consumers as the public proof system string for this release.
- Initial conformance vectors may use placeholder assets in source control, provided the harness structure and expected outcomes are fully defined and executable.
- Performance evaluation is performed on representative production-like workloads and envelopes up to 10KB.

## Clarifications

- No open clarifications. Scope and behavior are fully defined for verification-only SDK integration of Midnight as the first supported proof system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For a fixed set of conformance vectors and stable configuration, repeated validation runs produce identical verification outcomes in 100% of runs.
- **SC-002**: 100% of known-invalid Midnight proof vectors result in `Invalid`-mapped failures, with 0 mappings to `Expired` or `Unsupported`.
- **SC-003**: 100% of simulated internal verifier fault vectors result in `Error`-mapped failures.
- **SC-004**: In validation workloads with envelope sizes up to 10KB, end-to-end p95 validation time stays within the Spec 002 budget, and Midnight verification consumes no more than 60% of that p95 budget.
- **SC-005**: Security/diagnostic tests show 0 instances of raw `proofBytes` leakage in logs, exceptions, or diagnostic outputs across normal and failure paths.
- **SC-006**: Conformance suite includes at least three required vectors at release: one known-valid `license:v1` vector, one known-invalid vector, and one simulated internal-error vector.
