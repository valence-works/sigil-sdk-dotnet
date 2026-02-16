# Feature Specification: SDK Validation API

**Feature Branch**: `003-sdk-validation-api`  
**Created**: 2026-02-16  
**Status**: Draft  
**Input**: User description: "Create a feature specification for SDK Validation API for Sigil SDK. The API must validate Proof Envelopes defined in Spec 001. Requirements: Result-object API (no exceptions for validation failures); LicenseStatus enum with Valid, Invalid, Expired, Unsupported, Malformed, Error; deterministic failure codes; validate JSON schema before cryptographic verification; extract proofSystem and statementId early and enforce DI-based immutable registries; evaluate expiresAt and return Expired when appropriate; compile JSON schema once at startup; provide ValidateAsync(string) and ValidateAsync(Stream); structured logging policy (never log proofBytes; diagnostics opt-in only); fail closed. Include measurable success criteria and acceptance scenarios."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate a Proof Envelope safely (Priority: P1)

As an application developer embedding Sigil SDK, I want to validate a Proof Envelope and receive a structured result so my application can make allow/deny decisions without crashing.

**Why this priority**: This is the primary integration point for offline license proof verification.

**Independent Test**: Provide a syntactically valid Proof Envelope JSON string and confirm a deterministic result is returned with a stable status and failure code.

**Acceptance Scenarios**:

1. **Given** a Proof Envelope that conforms to Spec 001 and has a cryptographically valid proof for its `statementId` and `proofSystem`, **When** validation runs, **Then** the result status is `Valid` and includes extracted identifiers (`statementId`, `proofSystem`) and typed claims.
2. **Given** a Proof Envelope that conforms to Spec 001 but has an invalid proof, **When** validation runs, **Then** the result status is `Invalid` and includes a deterministic failure code explaining the reason.
3. **Given** a Proof Envelope that fails JSON parsing or schema validation, **When** validation runs, **Then** the result status is `Malformed` and validation does not attempt cryptographic verification.

---

### User Story 2 - Deterministic failures for policy decisions (Priority: P2)

As an application developer, I want stable failure codes and coarse-grained statuses so I can implement policy and telemetry that remains consistent across SDK versions.

**Why this priority**: Determinism prevents accidental behavior changes in license enforcement.

**Independent Test**: Validate the same invalid envelope twice and confirm identical status and failure code each time.

**Acceptance Scenarios**:

1. **Given** a malformed JSON envelope, **When** validation runs repeatedly, **Then** the same failure code is returned every time.
2. **Given** an expired envelope, **When** validation runs, **Then** the result status is `Expired` (not `Invalid`).
3. **Given** an envelope with an unknown `proofSystem` or unknown `statementId`, **When** validation runs, **Then** the result status is `Unsupported` with a deterministic failure code.

---

### User Story 3 - Secure diagnostics and logging (Priority: P3)

As a security-conscious integrator, I want logging to be safe by default and diagnostics to be opt-in so sensitive proof material is not leaked.

**Why this priority**: Proof material may be sensitive; leaking it can undermine security and privacy.

**Independent Test**: Enable validation with default settings and confirm logs contain no `proofBytes` content; enable diagnostics explicitly and confirm additional diagnostic details are still safe.

**Acceptance Scenarios**:

1. **Given** any envelope (valid or invalid), **When** validation logs events, **Then** logs MUST NOT include `proofBytes` or any representation that can be used to reconstruct it.
2. **Given** diagnostics are not enabled, **When** an internal exception occurs, **Then** the result status is `Error` with a deterministic failure code and the thrown exception is not attached to the returned result.

### Edge Cases

- Envelope JSON is not a JSON object (e.g., array, number).
- Envelope is missing required Spec 001 fields.
- Envelope includes invalid base64 for `proofBytes`.
- `expiresAt` is present but not a valid timestamp.
- `expiresAt` is present and already in the past (must return `Expired`).
- `proofSystem` / `statementId` are missing, empty, or not strings.
- `proofSystem` / `statementId` are present but not registered (must return `Unsupported`).
- Schema validation passes but cryptographic verification fails (must return `Invalid`).
- Any unexpected internal error during validation (must fail closed with `Error`).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The SDK MUST validate Proof Envelopes that conform to Spec 001 (Proof Envelope Format).
- **FR-002**: The validation API MUST be result-object based; normal validation failures MUST NOT be surfaced as exceptions to the caller.
- **FR-003**: The SDK MUST expose a `LicenseStatus` model with exactly these values: `Valid`, `Invalid`, `Expired`, `Unsupported`, `Malformed`, `Error`.
- **FR-004**: The SDK MUST return deterministic failure codes for all non-`Valid` outcomes; the same input MUST yield the same status and failure code.
- **FR-005**: Validation MUST validate the envelope against the Spec 001 JSON schema before performing any cryptographic verification.
- **FR-006**: Validation MUST extract `proofSystem` and `statementId` as early as practical (before registry resolution and before cryptographic verification) to support deterministic routing and diagnostics.
- **FR-007**: Validation MUST enforce immutable, dependency-injection-provided registries for statement handlers and proof system verifiers; registry contents MUST NOT change at runtime.
- **FR-008**: Validation MUST evaluate `expiresAt` (from Spec 001 public inputs) and return status `Expired` when `expiresAt` is present and is earlier than current UTC time.
- **FR-009**: Validation MUST compile/initialize the Spec 001 JSON schema once at process startup (or equivalent application start) and reuse it for subsequent validations.
- **FR-010**: The SDK MUST provide two asynchronous validation entry points: `ValidateAsync(string)` and `ValidateAsync(Stream)`.
- **FR-011**: The SDK MUST implement a structured logging policy where `proofBytes` are never logged; additional diagnostics MUST be opt-in.
- **FR-012**: The SDK MUST fail closed: it MUST NOT return `Valid` unless all required validation steps succeed, and any unhandled/unexpected condition MUST return a non-`Valid` result.

#### Failure Model Requirements

- **FR-013**: The SDK MUST provide a stable set of failure codes that map deterministically to exactly one `LicenseStatus`.
- **FR-014**: `Malformed` MUST be used for JSON parsing failures, schema validation failures, and required-field/type violations.
- **FR-015**: `Unsupported` MUST be used when the envelope’s `envelopeVersion`, `proofSystem`, or `statementId` are syntactically valid but not supported/registered.
- **FR-016**: `Invalid` MUST be used when schema is valid and the envelope is supported, but cryptographic verification fails or required semantic checks fail.
- **FR-017**: `Error` MUST be used for unexpected internal failures (e.g., exceptions, resource failures) and MUST still be deterministic at the failure-code level.

### Key Entities *(include if feature involves data)*

- **Proof Envelope**: A versioned JSON document defined by Spec 001, containing `envelopeVersion`, `proofSystem`, `statementId`, `proofBytes`, and public inputs.
- **Validation Result**: A structured response containing `LicenseStatus`, optional extracted identifiers, optional typed claims, and an optional failure object.
- **Failure Code**: A stable identifier representing a specific validation failure reason, suitable for application policy and telemetry.
- **Immutable Registries**: Application-provided registries of supported statement IDs and proof systems used to route validation.

## Assumptions

- Validation operates fully offline (no network calls) for the validation path.
- “Current time” comparisons for expiry use current UTC time.
- Spec 001 provides a machine-verifiable JSON schema contract for envelope structure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For a fixed input envelope, validation returns the same `LicenseStatus` and failure code across 100 repeated runs.
- **SC-002**: 0 occurrences of `proofBytes` content appearing in logs when running the SDK’s validation on a representative suite of valid and invalid envelopes.
- **SC-003**: Expired envelopes (where `expiresAt` is in the past) are classified as `Expired` with a deterministic failure code in 100% of tested cases.
- **SC-004**: Schema-invalid envelopes do not trigger cryptographic verification attempts in 100% of tested cases.
- **SC-005**: Callers can validate from both a JSON string and a stream input, achieving functional equivalence in outcomes for the same envelope content.

## Acceptance Scenarios (Cross-cutting)

1. **Given** an envelope that fails schema validation, **When** validation runs, **Then** status is `Malformed` and failure code indicates schema violation.
2. **Given** an envelope that is schema-valid but references an unregistered `proofSystem`, **When** validation runs, **Then** status is `Unsupported` and failure code indicates unknown proof system.
3. **Given** an envelope that is schema-valid, supported, and cryptographically valid but expired, **When** validation runs, **Then** status is `Expired`.
4. **Given** an envelope that is schema-valid, supported, and unexpired but cryptographically invalid, **When** validation runs, **Then** status is `Invalid`.
5. **Given** an unexpected internal error occurs during validation, **When** validation runs, **Then** status is `Error` with a deterministic failure code and no exception is required for the caller to handle.
