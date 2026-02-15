# Feature Specification: Proof Envelope Format

**Feature Branch**: `001-proof-envelope-format`  
**Created**: 2026-02-15  
**Status**: Draft  
**Input**: User description: "Create a feature specification for Proof Envelope Format for Sigil SDK. It must define a versioned JSON envelope for offline license proof verification. The envelope must include: envelopeVersion (string, versioned), proofSystem (identifier of verification mechanism), statementId (URN identifying semantic contract), proofBytes (base64-encoded), publicInputs (fixed schema: productId, edition, features lowercase kebab-case, expiresAt, maxSeats), optional issuedAt, optional policyHash, optional extensions (forward-compatible). Requirements: Offline verification only, forward-compatible top-level structure, publicInputs strict for v1, unknown proofSystem or statementId must fail deterministically, expiry must be evaluated and expired licenses must fail, fail-closed security model, never log proofBytes. Include measurable success criteria and acceptance scenarios."

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

### User Story 1 - Define a proof envelope (Priority: P1)

As a product or licensing team, I want a versioned JSON envelope so I can issue offline proofs that verifiers validate consistently.

**Why this priority**: The envelope format is the foundation for all offline verification.

**Independent Test**: Create a minimal envelope with required fields and confirm it is accepted as valid format.

**Acceptance Scenarios**:

1. **Given** an envelope with all required fields and valid values, **When** it is parsed, **Then** it is accepted as a valid Proof Envelope Format document.
2. **Given** an envelope missing a required field, **When** it is parsed, **Then** it is rejected with a deterministic failure reason.

---

### User Story 2 - Enforce strict public inputs (Priority: P2)

As a verifier, I want `publicInputs` to be strict for v1 so I can rely on consistent claim structure.

**Why this priority**: Strict inputs prevent silent acceptance of unexpected claims.

**Independent Test**: Validate envelopes with extra `publicInputs` fields and confirm they fail.

**Acceptance Scenarios**:

1. **Given** an envelope with an unknown field inside `publicInputs`, **When** validation runs, **Then** it fails deterministically.

---

### User Story 3 - Forward-compatible top-level envelope (Priority: P3)

As an integrator, I want to add top-level fields without breaking existing verifiers.

**Why this priority**: Future metadata should not require coordinated upgrades.

**Independent Test**: Validate envelopes with unknown top-level fields and confirm acceptance when required fields are valid.

**Acceptance Scenarios**:

1. **Given** an envelope with extra top-level fields, **When** it is validated, **Then** those fields are ignored and the result depends only on known fields.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- `proofBytes` is empty or not valid base64.
- `statementId` is not a URN or is empty.
- `features` contains non-kebab-case entries or duplicates.
- `expiresAt` is present and already expired.
- `publicInputs` is missing required fields.
- Unknown top-level fields are present.

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

- **FR-001**: The envelope MUST be a JSON object with `envelopeVersion` identifying the format version.
- **FR-001a**: For v1, `envelopeVersion` MUST equal "1.0".
- **FR-002**: The envelope MUST include `proofSystem`, `statementId`, `proofBytes`, and `publicInputs`.
- **FR-003**: `statementId` MUST be a URN identifying the semantic contract.
- **FR-004**: `proofBytes` MUST be base64-encoded and non-empty.
- **FR-005**: `publicInputs` MUST contain only the fixed schema fields: `productId`, `edition`, `features`, optional `expiresAt`, optional `maxSeats`.
- **FR-006**: `publicInputs` MUST be strict for v1; unknown fields MUST cause validation to fail.
- **FR-007**: `features` MUST be a unique list of lowercase kebab-case identifiers.
- **FR-008**: `issuedAt`, `expiresAt` MUST be valid timestamps when present.
- **FR-009**: `policyHash` and `extensions` MUST be optional and MAY be omitted.
- **FR-010**: The top-level envelope MUST be forward-compatible; unknown top-level fields MUST be ignored for validation.
- **FR-011**: Validation MUST be offline and MUST NOT require network calls.
- **FR-012**: Unknown `proofSystem` or `statementId` MUST fail deterministically with a stable failure category.
- **FR-013**: When `expiresAt` is present and in the past relative to current UTC time, validation MUST fail with failure code `Expired`.
- **FR-014**: Validation MUST be fail-closed; any schema or integrity violation MUST result in rejection.
- **FR-015**: Validation MUST NOT log or emit `proofBytes` values.

### Key Entities *(include if feature involves data)*

- **Proof Envelope**: Versioned JSON document containing proof metadata, proof bytes, and public inputs.
- **Public Inputs**: Fixed schema claims that bind the proof to a product, edition, features, and optional limits.
- **Policy Reference**: Optional identifier linking to the issuance policy snapshot.

## Assumptions

- URN formatting follows common URN conventions.
- Time validation uses standard ISO 8601 timestamps.

## Clarifications

### Session 2026-02-15

- Q: What exact `envelopeVersion` value should v1 require? → A: "1.0"
- Q: How is `expiresAt` compared for expiry evaluation? → A: Compare to current UTC time.
- Q: What failure code name should represent expiry? → A: `Expired`

## Constitution Check

- Schema validation MUST run before proof verification. **PASS** (format requires schema validation; proof verification is out of scope).
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (FR-012).
- Validation failures MUST return a result object (no throws). **N/A** (format-only feature).
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (FR-015).
- Registries MUST be DI-configured and immutable after construction. **N/A** (format-only feature).
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (v1.0 defined; no breaking changes).

## Security Considerations

### Threat Model Summary

- Adversary can tamper with envelope fields to bypass validation.
- Adversary can craft oversized or malformed inputs to trigger parser failures.
- Sensitive proof material could leak through logs or diagnostics.

### Mitigations

- Fail closed on any schema or integrity violation (FR-014).
- Enforce strict `publicInputs` for v1 to prevent unmodeled claims (FR-006).
- Disallow logging or emission of `proofBytes` (FR-015).
- Require deterministic failure handling for unknown identifiers (FR-012).

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: 100% of envelopes missing required fields are rejected with the same failure category on repeated runs.
- **SC-002**: 100% of envelopes with unknown `proofSystem` or `statementId` are rejected deterministically.
- **SC-003**: 95% of valid envelopes of size up to 10 KB validate in under 1 second offline.
- **SC-004**: 0 validation logs contain `proofBytes` content during acceptance testing.
- **SC-005**: 100% of expired envelopes return an expired failure outcome.
