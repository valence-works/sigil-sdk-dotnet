# Feature Specification: Proof System Verifier Contract

**Feature Branch**: `005-proof-system-verifier-contract`  
**Created**: 2026-02-20  
**Status**: Draft  
**Input**: User description: "Create Spec 005: Proof System Verifier Contract for the Sigil SDK (verification-only repo), with deterministic, fail-closed, midnight-first verifier behavior and DI-based extensibility."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify supported proofs deterministically (Priority: P1)

As an SDK integrator, I want proof verification to return deterministic outcomes for a supported proof system so I can make reliable allow/deny decisions in offline environments.

**Why this priority**: Deterministic verification is the core business value of the SDK and must work before all other extensibility concerns.

**Independent Test**: Register one supported proof verifier (`midnight-zk-v1`), submit identical valid and invalid inputs repeatedly, and confirm identical statuses and failure codes every time.

**Acceptance Scenarios**:

1. **Given** a registered verifier for `midnight-zk-v1` and valid verification context from the selected statement handler, **When** the same `proofBytes` and context are verified multiple times under identical registry configuration, **Then** the outcome is identical each time.
2. **Given** a registered verifier for `midnight-zk-v1`, **When** verification fails cryptographically, **Then** the validator returns status `Invalid` with a deterministic failure code and does not return `Expired` or `Unsupported`.
3. **Given** an environment without network connectivity, **When** verification is executed for `midnight-zk-v1`, **Then** verification still completes because no network dependency is required.

---

### User Story 2 - Resolve proof systems through immutable DI registries (Priority: P2)

As an application developer, I want proof system verifiers resolved from immutable DI registration so the active verification behavior is explicit, stable, and extensible.

**Why this priority**: Reliable registration and resolution are required for both current Midnight support and future proof-system additions.

**Independent Test**: Build the registry from DI with known entries, validate envelopes for supported and unsupported `proofSystem` identifiers, and confirm deterministic resolution behavior.

**Acceptance Scenarios**:

1. **Given** a DI configuration with exactly one verifier per `proofSystem`, **When** validation resolves a supported identifier, **Then** it selects the corresponding verifier deterministically.
2. **Given** an envelope with an unknown or unregistered `proofSystem`, **When** validation runs, **Then** it fails deterministically with status `Unsupported` according to Spec 002.
3. **Given** duplicate verifier registrations for the same `proofSystem`, **When** the immutable registry is built, **Then** the configuration is rejected deterministically and validation does not proceed with ambiguous routing.

---

### User Story 3 - Preserve fail-closed pipeline semantics and safe diagnostics (Priority: P3)

As a security reviewer, I want proof verification to preserve first-failing-stage behavior and redact proof material so failures are safe, bounded, and auditable.

**Why this priority**: Safety and deterministic stage ordering prevent misclassification and sensitive data exposure.

**Independent Test**: Execute malformed, unsupported, crypto-invalid, and internal-error scenarios with logging enabled and verify status mapping, stage precedence, and redaction guarantees.

**Acceptance Scenarios**:

1. **Given** a malformed envelope that fails schema validation, **When** validation runs, **Then** proof verification is not executed and schema failure remains the returned result.
2. **Given** an envelope with unsupported identifiers, **When** validation runs, **Then** `Unsupported` is returned and proof verification does not override that failure.
3. **Given** an internal verifier runtime error, **When** validation runs, **Then** status `Error` is returned with deterministic error classification and without exposing `proofBytes`.
4. **Given** diagnostics are enabled, **When** verification fails, **Then** logs/diagnostics do not include `proofBytes` at any level or in exception messages.

### Edge Cases

- `proofBytes` is empty, null-equivalent, or corrupted while schema and identifiers are otherwise valid.
- Verification context from the statement handler is structurally valid but semantically incompatible with the targeted proof system.
- The same envelope is validated concurrently under the same immutable registry configuration.
- The verifier returns a non-success signal without additional details; validator still maps outcome deterministically.
- Cancellation occurs during proof verification.
- A timeout policy (if present at caller level) cancels verification and is observed as cancellation, not a validation status.
- Diagnostics are disabled; validation still returns deterministic statuses without relying on diagnostic side channels.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The SDK MUST define a proof system verifier abstraction that is independent of statement semantics and can be implemented for `midnight-zk-v1` and future proof systems.
- **FR-002**: Proof verification MUST accept `proofBytes` and statement-specific verification context produced by the resolved statement handler.
- **FR-003**: The proof verifier contract MUST NOT define or reinterpret statement meaning; statement semantics remain the responsibility of statement handlers and validator orchestration.
- **FR-004**: Proof verification MUST be executable offline and MUST NOT require network access to complete.
- **FR-005**: For identical `proofBytes`, identical statement verification context, and identical immutable registry/configuration inputs, the verification outcome MUST be deterministic.

- **FR-006**: Proof system verifiers MUST be registered through dependency injection into an immutable proof-system registry.
- **FR-007**: The immutable proof-system registry MUST contain at most one verifier per canonical `proofSystem` identifier.
- **FR-007a**: `proofSystem` identifier resolution MUST use exact, case-sensitive canonical matching; case-variant inputs MUST NOT be normalized and MUST fail as `Unsupported` when no exact match exists.
- **FR-008**: If `proofSystem` is unknown or unregistered, validation MUST fail deterministically with status `Unsupported` and a deterministic failure code aligned with Spec 002.
- **FR-009**: Registry configuration that results in duplicate entries for the same `proofSystem` MUST fail deterministically before runtime verification begins.

- **FR-010**: A cryptographic verification failure for a supported proof system MUST map to status `Invalid` and MUST NOT map to `Expired` or `Unsupported`.
- **FR-010a**: For a supported proof system, incompatibility between `proofBytes` and statement-provided verification context MUST map to status `Invalid` with a deterministic failure code.
- **FR-011**: Internal verifier execution errors (for example, unexpected runtime faults) MUST map to status `Error` with deterministic error classification.
- **FR-011a**: Cancellation during proof verification MUST be propagated as cancellation to the caller and MUST NOT be remapped to validation statuses (`Invalid`, `Unsupported`, `Malformed`, `Expired`, or `Error`).
- **FR-012**: Pipeline stage precedence MUST follow “first failing stage wins” as defined in Spec 002.
- **FR-013**: Proof verification MUST NOT override earlier schema, envelope-version, proof-system, or statement-resolution failures.
- **FR-014**: The validator MUST fail closed: it MUST return `Valid` only when all required stages, including proof verification, succeed.

- **FR-015**: The SDK MUST never log, emit, or include raw `proofBytes` in logs, exceptions, diagnostics, or returned error detail fields.
- **FR-016**: If diagnostics are enabled, they MUST redact proof material and only expose minimal metadata required for deterministic troubleshooting.
- **FR-017**: Verification output from this contract MUST be limited to verification success/failure and deterministic failure classification; semantic claim interpretation MUST remain outside proof-system verifiers.

- **FR-018**: The initial built-in proof-system support for this feature MUST cover `midnight-zk-v1`.
- **FR-019**: Adding a new proof system MUST be possible through registration of an additional verifier implementation without changing existing statement definitions.
- **FR-020**: The specification MUST remain verification-only and MUST NOT introduce issuance workflows, key-generation workflows, or platform-specific operational concerns.

### Key Entities *(include if feature involves data)*

- **Proof System Verifier**: A component responsible only for validating that `proofBytes` satisfy a given proof system using provided verification context.
- **Proof System Identifier**: Canonical identifier string (for example, `midnight-zk-v1`) used to resolve exactly one verifier from the immutable registry.
- **Verification Context**: Statement-scoped, precomputed context passed from the statement handler to the proof system verifier; it contains statement-specific verification inputs but not statement semantics.
- **Verification Outcome**: Deterministic result from proof verification used by the pipeline to map to Spec 002 statuses and failure codes.
- **Proof Verifier Registry**: Immutable runtime catalog mapping proof system identifiers to verifier implementations.

## Assumptions

- Spec 001 envelope parsing/schema requirements and Spec 002 pipeline ordering are already in effect for this feature.
- Statement handlers are responsible for constructing verification context compatible with the selected proof system.
- Performance targets are measured in offline execution mode on representative SDK host hardware.
- Determinism is evaluated under identical inputs and immutable registry/configuration state.
- Failure code taxonomy from Spec 002 is authoritative; this spec only defines mapping behavior for proof verification stages.

## Clarifications

### Session 2026-02-20

- Q: How should `proofSystem` identifier matching behave during verifier resolution? → A: Use exact case-sensitive canonical matching only; case variations fail as `Unsupported`.
- Q: How should cancellation/timeouts during proof verification be handled? → A: Propagate cancellation to the caller and do not map it to validation status codes.
- Q: How should proof/context incompatibility be classified for supported proof systems? → A: Return `Invalid` with a deterministic failure code.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In conformance testing, 100% of repeated validations of identical inputs under identical immutable registry configuration produce identical status and failure code results.
- **SC-002**: 100% of envelopes with unknown/unregistered `proofSystem` return `Unsupported` deterministically.
- **SC-003**: 100% of supported-proof cryptographic failures return `Invalid`, and 0% are misclassified as `Expired` or `Unsupported`.
- **SC-004**: 100% of internal verifier runtime faults return `Error` and preserve first-failing-stage semantics.
- **SC-005**: 0 logs, diagnostics, exception messages, or returned error details contain raw `proofBytes` during acceptance testing.
- **SC-006**: For envelopes up to 10 KB, end-to-end SDK validation (including proof verification) completes within 1 second at p95 in offline conformance benchmarks.
- **SC-007**: A second proof system can be added through DI registration and pass full conformance scenarios without requiring changes to existing statement definitions.
