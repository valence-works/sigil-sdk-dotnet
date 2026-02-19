# Feature Specification: Statement Handler Contract & license:v1 Statement Definition

**Feature Branch**: `004-statement-handler-contract`  
**Created**: 2026-02-18  
**Status**: Draft  
**Input**: User description: "Spec 004 — Statement Handler Contract + license:v1 Statement Definition. Goal: formalize statement extensibility in code + spec. Includes: IStatementHandler / statement definition contract, v1 handler rules (publicInputs shape, claim extraction), expiry evaluation rule belongs to statement semantics (still executed by validator)"

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

### User Story 1 - Understand the statement concept and handler contract (Priority: P1)

As a developer or SDK maintainer, I want to understand what a statement is and how statement handlers validate them so I can implement custom statement handlers for application-specific claims.

**Why this priority**: The statement concept and handler contract are foundational to the extensibility model. Without a clear definition, developers cannot properly implement custom statements or understand how statements interact with the validator.

**Independent Test**: Read the specification and confirm you understand: (1) what a statement is, (2) the contract a statement handler must fulfill, (3) how statement handlers are invoked by the validator, (4) what output a statement handler must produce.

**Acceptance Scenarios**:

1. **Given** the statement concept is defined in the specification, **When** a developer reads the definition, **Then** they understand that a statement is a semantic interpretation of proof claim semantics specific to an application domain.
2. **Given** the handler contract is defined, **When** a developer implements a custom statement handler, **Then** their handler accepts publicInputs (JSON), validates their shape and values, optionally evaluates state-specific rules, and returns claims or a validation failure.
3. **Given** the handler contract includes the role of the validator, **When** a developer reads the contract, **Then** they understand which validation responsibilities belong to the handler and which belong to the validator (e.g., expiry evaluation).

---

### User Story 2 - Implement the built-in license:v1 statement handler (Priority: P2)

As a Sigil SDK maintainer, I want to define the license:v1 statement with specific publicInputs shape and claim extraction rules so that Sigil license verification works consistently across all integrations.

**Why this priority**: The license:v1 statement is the first and most common statement type. Formalizing its contract in the spec ensures consistency, enables predictable claim extraction for applications, and provides a reference implementation for custom statement handlers.

**Independent Test**: Provide the license:v1 handler with valid publicInputs matching the v1 schema and confirm it extracts all expected claims. Then provide publicInputs with missing or malformed fields and confirm deterministic validation failure.

**Acceptance Scenarios**:

1. **Given** publicInputs with all required v1 fields (productId, edition, features, expiresAt, maxSeats), **When** license:v1 handler validates them, **Then** it succeeds and extracts claims for productId, edition, features (as list), expiresAt, and maxSeats.
2. **Given** publicInputs missing required fields, **When** license:v1 handler validates them, **Then** it fails with a deterministic validation reason (e.g., "productId is required").
3. **Given** publicInputs with malformed field types (e.g., expiresAt is a string instead of a number), **When** license:v1 handler validates them, **Then** it fails deterministically without throwing an exception.
4. **Given** publicInputs with unknown additional fields (strict schema), **When** license:v1 handler validates them, **Then** it rejects them with a deterministic failure reason (e.g., "Unknown field: customField").

---

### User Story 3 - Clarify expiry evaluation as statement semantics (Priority: P3)

As an architect, I want the specification to clarify that expiry evaluation is part of statement semantics (where the decision of "is this proof valid?" is made) so that it's clear this responsibility belongs to the statement handler conceptually, even if the validator executes it as a separate pipeline stage.

**Why this priority**: Clarifying the conceptual ownership of expiry evaluation prevents confusion about where responsibility for different validation rules lies. It enables developers to understand the full context of what statements do, even if the validator's implementation manages expiry evaluation separately as an optimization.

**Independent Test**: Read the specification and the validator's code to confirm that: (1) expiry evaluation is described as part of statement semantics in the spec, (2) expiry evaluation is implemented as a separate stage in the validator's pipeline, (3) the spec document explains why this separation exists and how it fits into the overall validation flow.

**Acceptance Scenarios**:

1. **Given** the specification describes expiry evaluation in the context of statement semantics, **When** a developer understands the validator's implementation, **Then** they recognize that the validator executes expiry evaluation as a separate stage but it remains conceptually part of statement validation.
2. **Given** the validator's failure codes and documentation reference expiry evaluation in relation to statement validation, **When** a developer integrates the SDK, **Then** they understand that an "Expired" failure is a result of statement validation (expiry being part of statement semantics) rather than a separate orthogonal check.

---

### Edge Cases

- publicInputs is not a valid JSON object.
- publicInputs is null or empty.
- Individual claim values have wrong types (e.g., expiresAt is a string instead of a number).
- Required fields are missing or are null.
- features array contains duplicates or non-kebab-case values.
- expiresAt is present and in the past.
- maxSeats is 0 or negative.
- productId is empty or contains invalid characters.
- edition is empty or contains whitespace.
- A custom statement handler returns null claims when validation succeeds.
- A custom statement handler modifies publicInputs before validation.

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

#### Statement Concept & Handler Contract

- **FR-001**: The specification MUST define a statement as a semantic interpretation of proof claims within a specific application domain, enabling extensibility without breaking existing validation logic.
- **FR-002**: The specification MUST formalize the IStatementHandler contract so that developers can implement custom statement handlers:
  - Accept `publicInputs` (JSON) and a cancellation token
  - Validate the shape, types, and values of publicInputs according to statement-specific rules
  - Return a `StatementValidationResult` with an `IsValid` flag and optional `LicenseClaims`
  - Respect cooperative cancellation: check the `CancellationToken` and throw `OperationCanceledException` if cancellation is signaled before completing validation
  - Never throw exceptions for validation failures; use result objects instead
  - Support testability through deterministic outputs for the same inputs
- **FR-003**: The handler contract MUST specify which validation checks belong to the handler (shape/type validation, statement-specific rules) versus which belong to the validator (schema validation, expiry evaluation).
- **FR-004**: Statement handlers MUST use immutable registries registered via DI, preventing runtime modifications to the validation logic after startup.
- **FR-005**: The specification MUST document the failure modes and error conditions that a statement handler can report when `publicInputs` validation fails.
- **FR-005A**: Each statement handler MUST expose a canonical `StatementId` (URN), and statement registry keys MUST be derived from that `StatementId` to prevent ambiguous or duplicate registrations.

#### license:v1 Statement Definition

- **FR-006**: The specification MUST formally define the license:v1 statement type with specific rules for publicInputs validation and claim extraction.
- **FR-007**: The license:v1 statement handler MUST require publicInputs to have the following required fields:
  - `productId` (string, non-empty)
  - `edition` (string, non-empty)
  - `features` (array of strings, each in lowercase kebab-case, no duplicates)
  - `expiresAt` (integer, unix timestamp in seconds)
  - `maxSeats` (integer, positive)
- **FR-008**: The license:v1 statement handler MUST support optional publicInputs fields:
  - `issuedAt` (integer, unix timestamp in seconds)
  - `metadata` (JSON object, any structure, not validated)
- **FR-009**: The license:v1 statement handler MUST reject publicInputs that:
  - Are missing required fields
  - Have required fields with null values
  - Have required fields with incorrect types (e.g., expiresAt as string instead of number)
  - Have features as non-array or containing duplicates or non-kebab-case values
  - Have maxSeats as zero or negative
  - Have additional unknown fields (strict validation: any fields beyond the documented set are rejected with a deterministic failure reason)
- **FR-010**: The license:v1 statement handler MUST extract claims for:
  - `productId` (string)
  - `edition` (string)
  - `features` (array of strings)
  - `expiresAt` (unix timestamp)
  - `maxSeats` (integer)
  - `issuedAt` (unix timestamp, only if present in publicInputs)
  when validation succeeds. Extracted claims MUST match the validated publicInputs exactly.

#### Expiry Evaluation in Statement Semantics

- **FR-011**: The specification MUST clarify that expiry evaluation is conceptually part of statement validation (statement semantics), even though the validator may implement it as a separate pipeline stage.
- **FR-012**: The validator MUST evaluate `expiresAt` from public inputs and determine if the proof is expired (expiresAt is in the past relative to current time).
- **FR-013**: The validator MUST return a deterministic failure with failure code "Expired" when a valid proof's `expiresAt` has passed, without reporting this as a statement validation error per se.
- **FR-014**: Expiry evaluation MUST occur after statement validation succeeds, so that malformed publicInputs fail before expiry is checked.

#### Immutability & Determinism

- **FR-015**: All statement handlers registered in the immutable statement registry MUST be immutable after DI container initialization (Spec 003 requirement, maintained here).
- **FR-016**: For the same publicInputs, a statement handler MUST return the same `StatementValidationResult` every time (no randomness, no side effects from external state).
- **FR-017**: The specification MUST require that statement handler implementations never log `publicInputs` in their entirety if diagnostics are enabled; sensitive values MUST be omitted or redacted.
- **FR-018**: Statement handlers MUST NOT return `StatementValidationResult(IsValid=true, Claims=null)`. If validation succeeds, claims MUST be populated with extracted values. A handler returning null claims on success is an implementation error and MUST be treated as a validation failure by the validator.

### Key Entities

- **Statement**: A semantic definition of how to validate and extract claims from publicInputs. Defined by a unique `statementId` (URN). Examples: `urn:sigil:statement:license:v1`.
  
- **Statement Handler** (`IStatementHandler`): The implementation that validates publicInputs according to a specific statement's rules and extracts claims. Registered in the immutable statement registry.
  
- **license:v1 Statement**: The built-in statement provided by Sigil SDK for validating Sigil license proofs. Specifies the required shape of publicInputs (productId, edition, features, expiresAt, maxSeats) and the claim extraction logic.
  
- **PublicInputs**: A JSON object embedded in the Proof Envelope that contains the claims sealed in the zero-knowledge proof. Must be validated against the chosen statement's rules.
  
- **LicenseClaims**: The structured output of successful statement validation, containing validated and extracted claims: productId (string), edition (string), features (array of strings), expiresAt (unix timestamp), maxSeats (integer), and optionally issuedAt (unix timestamp if present in publicInputs).

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: Developers can implement a custom statement handler by implementing `IStatementHandler` and registering it via DI, requiring 5-10 lines of code for a simple statement.
- **SC-002**: The license:v1 statement handler correctly validates 100% of well-formed publicInputs and rejects 100% of malformed publicInputs with deterministic failure reasons.
- **SC-003**: Statement validation completes in under 10ms for publicInputs with up to 1000 features entries.
- **SC-004**: Custom statement handlers can be registered and executed without modifying Sigil SDK source code, demonstrating extensibility.
- **SC-005**: The specification document clearly distinguishes between statement handler responsibilities and validator responsibilities, as confirmed by developer review (0 clarification questions on typical integration).
- **SC-006**: 100% of statement handlers are deterministic: the same publicInputs always produce the same StatementValidationResult.
- **SC-007**: All documentation examples (spec + code comments) are executable and pass validation without modification.

## Assumptions

- **A-001**: Custom statement handlers are registered at startup and do not change at runtime (immutability is enforced).
- **A-002**: publicInputs are always JSON objects (never null, never non-object types); the validator ensures this before passing to the handler.
- **A-003**: Statement handlers do not need to assess whether a license is suspended, revoked, or otherwise administratively invalid; they only validate the structure and contents of publicInputs.
- **A-004**: Expiry evaluation uses the validator's system time, not a time stored in publicInputs (expiresAt is compared to DateTime.UtcNow).
- **A-005**: Features are validated as lowercase kebab-case by the statement handler; the issuer must ensure features are issued in this format.
- **A-006**: Unknown extra fields in publicInputs are rejected (strict schema), preventing accidental misconfigurations and catching issuer errors early. This is the chosen behavior per clarification response.
- **A-007**: The specification is written for developers implementing statement handlers or reviewing Sigil SDK internals; it assumes familiarity with JSON, .NET async/await, and the Proof Envelope format (Spec 001).

## Clarifications

### Session 2026-02-18

- Q: Should `issuedAt` be extracted as a claim? → A: Yes, extract issuedAt if present in publicInputs. Applied to FR-010 and LicenseClaims definition.
- Q: How should statement handlers handle CancellationToken? → A: Respect cooperative cancellation (check token, throw OperationCanceledException if signaled). Applied to FR-002.
- Q: Should handlers be allowed to return null claims when validation succeeds? → A: No. If IsValid=true, claims must be populated. Applied to FR-018; edge case clarified.

## Constitution Check

This feature must comply with the Sigil SDK constitution in `.specify/memory/constitution.md`.

Key constraints relevant to this spec:
- **Deterministic validation**: Statement handlers must return the same result for the same inputs.
- **Result objects**: No exceptions for validation failures; use result objects.
- **Never log proofBytes**: Statement handlers should not log publicInputs in full.
- **Immutable registries**: Statement handlers are registered in immutable registries.
- **Offline verification**: Statement handlers operate on staticpublicInputs; no external state or network calls.
- **Fail-closed**: Invalid inputs fail validation; no silent acceptance of malformed data.

