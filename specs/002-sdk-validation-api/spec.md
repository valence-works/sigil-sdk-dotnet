# Feature Specification: SDK Validation API

**Feature Branch**: `002-sdk-validation-api`  
**Created**: 2026-02-15  
**Status**: Draft  
**Input**: User description: "Create a feature specification for SDK Validation API for Sigil SDK. Validation must return deterministic result objects (no exceptions for validation failures), include LicenseStatus + failure codes, validate schema before proof verification, resolve proofSystem and statementId via DI registries (immutable), evaluate expiresAt and fail with Expired when expired, compile schema once at startup, provide ValidateAsync(string) and ValidateAsync(Stream), and enforce logging/diagnostics policy (never log proofBytes; diagnostics opt-in)."

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
2. **Given** a valid envelope, **When** validation runs, **Then** the result object indicates success and includes the corresponding `LicenseStatus`.

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

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

<!--
  NOTE: Translate constitution constraints into explicit FRs when applicable
  (schema-before-proof, deterministic failures, no proofBytes logging,
  result-object validation errors, registry immutability, breaking-change docs).
-->

### Functional Requirements

- **FR-001**: Validation MUST return deterministic result objects for all inputs and MUST NOT throw exceptions for validation failures.
- **FR-002**: Each validation result MUST include a `LicenseStatus` and zero or more failure codes.
- **FR-003**: Validation MUST perform schema validation before any proof verification.
- **FR-004**: Validation MUST resolve `proofSystem` and `statementId` via immutable registries provided at startup.
- **FR-005**: Unknown `proofSystem` or `statementId` MUST fail deterministically with a stable failure code.
- **FR-006**: When `expiresAt` is present and in the past, validation MUST return `Expired` as the failure code.
- **FR-007**: The schema MUST be compiled once at startup and reused for all validations.
- **FR-008**: The API MUST provide `ValidateAsync(string)` and `ValidateAsync(Stream)` entry points.
- **FR-009**: Validation MUST be offline and MUST NOT require network calls.
- **FR-010**: Logging and diagnostics MUST never emit `proofBytes` content.
- **FR-011**: Diagnostics MUST be opt-in and disabled by default.

### Key Entities *(include if feature involves data)*

- **Validation Result**: Deterministic outcome containing status and failure codes for a validation run.
- **License Status**: User-facing status value that describes the license state derived from validation.
- **Failure Code**: Stable, enumerable reason for a validation failure.
- **Registry**: Immutable catalog for resolving `proofSystem` and `statementId` identifiers.

## Assumptions

- Registries are configured before validation begins and do not change at runtime.
- Timestamps use standard ISO 8601 format and are compared using a consistent clock source.
- Validation inputs are UTF-8 encoded when provided as strings or streams.

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: 100% of validation failures return deterministic result objects with stable failure codes.
- **SC-002**: 0 validation failures are surfaced as thrown exceptions in acceptance testing.
- **SC-003**: 100% of envelopes with expired `expiresAt` return `Expired` as the failure code.
- **SC-004**: Validation completes in under 1 second for 95% of envelopes up to 10 KB in size when offline.
- **SC-005**: 0 log or diagnostic records contain `proofBytes` content during acceptance testing.
