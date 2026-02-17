# Feature Specification: SDK Validation API

**Feature Branch**: `002-sdk-validation-api`  
**Created**: 2026-02-16  
**Status**: Draft  
**Input**: User description: "Create a feature specification for SDK Validation API for Sigil SDK. The API must validate Proof Envelopes defined in Spec 001. Requirements: Result-object API (no exceptions for validation failures); LicenseStatus enum with Valid, Invalid, Expired, Unsupported, Malformed, Error; deterministic failure codes; validate JSON schema before cryptographic verification; extract proofSystem and statementId early and enforce DI-based immutable registries; evaluate expiresAt and return Expired when appropriate; compile JSON schema once at startup; provide ValidateAsync(string) and ValidateAsync(Stream); structured logging policy (never log proofBytes; diagnostics opt-in only); fail closed. Include measurable success criteria and acceptance scenarios."

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Validate a proof envelope deterministically (Priority: P1)

As an integrator, I want a validation API that returns deterministic results so I can handle acceptance and rejection without exceptions for validation failures.

**Why this priority**: Deterministic result objects are required to build reliable integrations and error handling.

**Independent Test**: Validate the same input multiple times and confirm identical result objects and failure codes without exceptions.

**Acceptance Scenarios**:

1. **Given** an invalid envelope, **When** validation runs, **Then** a deterministic result object is returned and no exception is raised for validation failure.
2. **Given** a valid envelope, **When** validation runs, **Then** the result object indicates success and includes typed claims derived from verified `publicInputs`.

---

### User Story 2 - Enforce schema-first validation (Priority: P2)

As a verifier, I want schema validation to run before proof verification so I can fail fast and consistently when the envelope is malformed.

**Why this priority**: Schema-first validation reduces computational cost and yields clear failure categories.

**Independent Test**: Provide malformed envelopes and confirm schema validation failure without proof verification.

**Acceptance Scenarios**:

1. **Given** an envelope that violates the schema, **When** validation runs, **Then** it fails before any proof verification is attempted.

---

### User Story 3 - Support offline registry resolution (Priority: P3)

As an integrator, I want proof system and statement resolution to use immutable registries so validation is stable and offline.

**Why this priority**: Stable registry resolution prevents runtime drift and ensures deterministic behavior.

**Independent Test**: Validate with registries that have known entries and confirm unknown identifiers fail deterministically.

**Acceptance Scenarios**:

1. **Given** an envelope with an unknown `proofSystem` or `statementId`, **When** validation runs, **Then** it fails deterministically with a failure code.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- `expiresAt` is present and already expired.
- Schema is valid but proof verification fails.
- Input is empty or not valid JSON.
- Input stream is truncated.
- `proofBytes` is present but diagnostics are enabled.
- Registry entries are missing or duplicated at startup.

## Constitution Check

This feature must comply with the Sigil SDK constitution in `.specify/memory/constitution.md`.

- Schema validation runs before proof verification: **PASS** (FR-005).
- Unknown `proofSystem` or `statementId` fails deterministically: **PASS** (FR-006, FR-007, FR-013a, FR-015).
- Validation failures return a result object (no exceptions for expected failures): **PASS** (FR-002, FR-002a).
- `proofBytes` is never logged or emitted in diagnostics: **PASS** (FR-011).
- Registries are DI-configured and immutable after construction: **PASS** (FR-007).
- Fail-closed behavior (never return `Valid` unless all steps succeed): **PASS** (FR-012).

## Requirements *(mandatory)*


### Functional Requirements

- **FR-001**: The SDK MUST validate Proof Envelopes that conform to Spec 001 (Proof Envelope Format v1.0).
- **FR-002**: The validation API MUST be result-object based; normal validation failures MUST NOT be surfaced as exceptions to the caller.
- **FR-002a**: The SDK MAY throw exceptions only for programmer errors (e.g., null arguments) or gross misconfiguration; invalid inputs and failed checks MUST return a non-`Valid` result object.

  Gross misconfiguration is limited to cases where validation cannot run safely, such as:

  - Required services missing from DI (e.g., schema validator, registries, clock).
  - Options configuration that prevents safe operation.

  Misconfiguration exceptions SHOULD be deterministic and use standard .NET exception types (e.g., `InvalidOperationException`) and MUST NOT include `proofBytes`.
- **FR-003**: The SDK MUST expose a `LicenseStatus` model with exactly these values: `Valid`, `Invalid`, `Expired`, `Unsupported`, `Malformed`, `Error`.
- **FR-004**: Each validation result MUST include a `LicenseStatus` and, when not `Valid`, a deterministic failure code suitable for policy and telemetry.
- **FR-004a**: When `LicenseStatus` is not `Valid`, the result MUST include exactly one failure code (not a list).
- **FR-005**: Validation MUST validate the envelope against the Spec 001 JSON schema before performing any cryptographic verification. Identifier extraction and envelopeVersion support checks MAY occur prior to full schema validation to enable deterministic routing and failure classification.
- **FR-006**: Validation MUST extract `proofSystem` and `statementId` as early as practical (before registry resolution and before cryptographic verification) to support deterministic routing and diagnostics.
- **FR-007**: Validation MUST enforce immutable, dependency-injection-provided registries for statement handlers and proof system verifiers; registry contents MUST NOT change at runtime.
- **FR-007a**: Duplicate registry entries detected at startup MUST be treated as misconfiguration and MUST fail fast with a deterministic exception.
- **FR-008**: Validation MUST evaluate `publicInputs.expiresAt` (Spec 001) only after successful cryptographic verification / verified claim extraction.
- **FR-008a**: If cryptographic verification fails, validation MUST return status `Invalid` and MUST NOT return status `Expired`.
- **FR-008b**: If cryptographic verification succeeds and `expiresAt` is present and earlier than the current UTC time (as provided by the configured clock abstraction), validation MUST return status `Expired`.
- **FR-009**: Validation MUST compile/initialize the Spec 001 JSON schema once at process startup (or equivalent application start) and reuse it for subsequent validations.
- **FR-010**: The SDK MUST provide two asynchronous validation entry points: `ValidateAsync(string)` and `ValidateAsync(Stream)`.
- **FR-010a**: Both `ValidateAsync` overloads MUST accept an optional `CancellationToken`.
- **FR-011**: The SDK MUST implement a structured logging policy where `proofBytes` are never logged at any log level and are never included in exception messages; additional diagnostics MUST be explicitly enabled via configuration.
- **FR-012**: The SDK MUST fail closed: it MUST NOT return `Valid` unless all required validation steps succeed, and any unhandled/unexpected condition MUST return a non-`Valid` result.

#### Failure Model Requirements

- **FR-013**: The SDK MUST provide a stable set of failure codes that map deterministically to exactly one `LicenseStatus`.
- **FR-013a**: The failure code returned MUST represent the first failing stage in the validation pipeline.
- **FR-014**: `Malformed` MUST be used for JSON parsing failures, schema validation failures, and required-field/type violations.
- **FR-015**: `Unsupported` MUST be used when the envelope’s `envelopeVersion`, `proofSystem`, or `statementId` are syntactically valid but not supported/registered.
- **FR-016**: `Invalid` MUST be used when schema is valid and the envelope is supported, but cryptographic verification fails or required semantic checks fail.
- **FR-017**: `Error` MUST NOT be used for expected validation failures covered by other failure codes.
- **FR-017a**: If `ValidateAsync(Stream)` fails to read the input stream (IO errors, truncation, disposal during read), validation MUST return status `Error` with a deterministic failure code.

### Key Entities *(include if feature involves data)*

- **Validation Result**: Deterministic outcome containing status and failure codes for a validation run.
- **License Status**: User-facing status value that describes the license state derived from validation.
- **Failure Code**: Stable, enumerable reason for a validation failure.
- **Registry**: Immutable catalog for resolving `proofSystem` and `statementId` identifiers.

## Assumptions

- Registries are configured before validation begins and do not change at runtime.
- Timestamps use standard ISO 8601 format and are compared using a consistent clock source.
- Validation inputs are UTF-8 encoded when provided as strings or streams.

## Clarifications

### Session 2026-02-16

- Q: When should `expiresAt` be evaluated relative to cryptographic verification? → A: Evaluate only after successful cryptographic verification / verified claim extraction; if crypto fails, return `Invalid` (not `Expired`).
- Q: Should a non-`Valid` result include exactly one failure code or multiple? → A: Exactly one deterministic failure code per non-`Valid` result (first failing stage wins).
- Q: Should null inputs throw or return a failure result? → A: Throw `ArgumentNullException` for null inputs (programmer error).
- Q: How should `ValidateAsync(Stream)` handle stream read failures? → A: Return status `Error` with a deterministic failure code.
- Q: Should `ValidateAsync` accept cancellation? → A: Yes, both overloads accept an optional `CancellationToken`.

### Session 2026-02-17

- Q: How should duplicate registry entries at startup be handled? → A: Treat as misconfiguration; fail fast with a deterministic exception.

## Success Criteria *(mandatory)*


### Measurable Outcomes

- **SC-001**: 100% of validation failures return deterministic result objects with stable failure codes.
- **SC-002**: 0 validation failures are surfaced as thrown exceptions in acceptance testing.
- **SC-003**: 100% of verified envelopes with expired `expiresAt` return status `Expired` with a deterministic failure code.
- **SC-004**: Validation completes in under 1 second for 95% of envelopes up to 10 KB in size when offline.
- **SC-005**: 0 log or diagnostic records contain `proofBytes` content during acceptance testing.

**SC-004 Measurement Method**:

- Use a fixed corpus of representative envelopes up to 10 KB (mix of valid, schema-invalid, and crypto-invalid).
- Warm the process (e.g., run 10 validations) so one-time schema initialization is not counted against steady-state latency (FR-009).
- Measure wall-clock duration per `ValidateAsync` call; compute p95 over at least 100 measured validations.
- Ensure the test is offline (no network calls) and uses a consistent clock source.
