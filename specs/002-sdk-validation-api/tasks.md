# Tasks: SDK Validation API

**Input**: Design documents from specs/002-sdk-validation-api/
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md
**Tests**: Not requested by spec; focus on implementation tasks only.
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: - [x] T### [P?] [US?] Description with file path

- **[P]**: Can run in parallel (different files, no dependencies)
- **[US#]**: Which user story this task belongs to
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: SDK project wiring required before implementation.

- [x] T001 Ensure solution references SDK and tests in Sigil.Sdk.sln
- [x] T002 Configure package references and embedded schema resource in src/Sigil.Sdk/Sigil.Sdk.csproj
- [x] T003 [P] Copy Spec 001 schema to src/Sigil.Sdk/Contracts/proof-envelope.schema.json
- [x] T004 [P] Ensure test project references SDK in tests/Sigil.Sdk.Tests/Sigil.Sdk.Tests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core building blocks required by all user stories.
**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [x] T005 Add `LicenseStatus` enum in src/Sigil.Sdk/Validation/LicenseStatus.cs
- [x] T006 Add `LicenseFailureCode` enum with explicit numeric values in src/Sigil.Sdk/Validation/LicenseFailureCode.cs
- [x] T007 [P] Add `LicenseValidationFailure` model in src/Sigil.Sdk/Validation/LicenseValidationFailure.cs
- [x] T008 [P] Add `LicenseValidationResult` model with `IsValid` + failure-code invariant in src/Sigil.Sdk/Validation/LicenseValidationResult.cs
- [x] T009 [P] Add `LicenseClaims` placeholder in src/Sigil.Sdk/Validation/LicenseClaims.cs
- [x] T010 Add diagnostics options in src/Sigil.Sdk/Validation/ValidationOptions.cs
- [x] T011 Add clock abstraction in src/Sigil.Sdk/Time/IClock.cs and default in src/Sigil.Sdk/Time/SystemClock.cs
- [x] T012 Add structured logging helpers that never accept `proofBytes` in src/Sigil.Sdk/Logging/ValidationLogging.cs
- [x] T013 Add schema validator interface in src/Sigil.Sdk/Schema/IProofEnvelopeSchemaValidator.cs
- [x] T014 Implement Corvus schema validator with single schema load in src/Sigil.Sdk/Schema/ProofEnvelopeSchemaValidator.cs
- [x] T015 Add registry + handler interfaces in src/Sigil.Sdk/Registries/IProofSystemRegistry.cs, src/Sigil.Sdk/Registries/IStatementRegistry.cs, src/Sigil.Sdk/Proof/IProofSystemVerifier.cs, src/Sigil.Sdk/Statements/IStatementHandler.cs
- [x] T016 Implement immutable registries in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs and src/Sigil.Sdk/Registries/ImmutableStatementRegistry.cs
- [x] T017 Add envelope reader for early extraction in src/Sigil.Sdk/Envelope/ProofEnvelopeReader.cs

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Validate a proof envelope deterministically (Priority: P1) 🎯 MVP

**Goal**: Deterministic result-object validation without exceptions for validation failures.
**Independent Test**: Validate the same invalid input twice and confirm identical status + failure code with no exception.

- [x] T018 [P] [US1] Define validator interface in src/Sigil.Sdk/Validation/ILicenseValidator.cs
- [x] T019 [P] [US1] Implement failure-code mapping for first-failing-stage in src/Sigil.Sdk/Validation/FailureClassification.cs
- [x] T020 [US1] Implement `ValidateAsync(string, CancellationToken)` in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T021 [US1] Implement `ValidateAsync(Stream, CancellationToken)` with deterministic stream read failures in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T022 [US1] Throw `ArgumentNullException` for null inputs in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T023 [US1] Return safe failure messages (no `proofBytes` or raw JSON) in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T024 [US1] Implement cryptographic verification stage in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T025 [US1] Implement statement semantic validation in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T026 [US1] Evaluate `expiresAt` only after verification in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T027 [US1] Enforce fail-closed handling for unexpected exceptions in src/Sigil.Sdk/Validation/LicenseValidator.cs

**Checkpoint**: Callers receive deterministic results and failure codes.

---

## Phase 4: User Story 2 — Enforce schema-first validation (Priority: P2)

**Goal**: Schema validation runs before any registry or cryptographic verification.
**Independent Test**: Schema-invalid input returns `Malformed` without invoking crypto verification.

- [x] T028 [US2] Add validation stage ordering constants in src/Sigil.Sdk/Validation/ValidationPipeline.cs
- [x] T029 [US2] Run schema validation before registry/crypto in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T030 [US2] Register schema validator as singleton at startup in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs

**Checkpoint**: Schema-first validation guaranteed on all paths.

---

## Phase 5: User Story 3 — Support offline registry resolution (Priority: P3)

**Goal**: Immutable DI registries provide deterministic `Unsupported` failures for unknown identifiers.
**Independent Test**: With empty registries, schema-valid input returns `Unsupported` with deterministic failure code.

- [x] T031 [US3] Resolve `proofSystem` and `statementId` via registries in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T032 [US3] Register validator, registries, schema validator, and clock in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs
- [x] T033 [US3] Fail fast on duplicate registry entries in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs and src/Sigil.Sdk/Registries/ImmutableStatementRegistry.cs

**Checkpoint**: Registry-driven routing is deterministic and offline.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation alignment and cross-cutting validation rules.

- [x] T034 [P] Align public API contract with implementation in specs/002-sdk-validation-api/contracts/public-api.md
- [x] T035 [P] Align failure code contract with implementation in specs/002-sdk-validation-api/contracts/failure-codes.md
- [x] T036 [P] Align logging policy with implementation in specs/002-sdk-validation-api/contracts/logging-policy.md
- [x] T037 [P] Update quickstart usage to match API in specs/002-sdk-validation-api/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup completion
- **User Stories (Phase 3–5)**: Depend on Foundational completion
- **Polish (Phase 6)**: Depends on desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only
- **US2 (P2)**: Depends on US1 (adds schema-first gating)
- **US3 (P3)**: Depends on US1 (adds registry resolution and immutability)

---

## Parallel Execution Examples

### US1

Task: "T018 Define validator interface in src/Sigil.Sdk/Validation/ILicenseValidator.cs"
Task: "T019 Implement failure-code mapping in src/Sigil.Sdk/Validation/FailureClassification.cs"

### US2

Task: "T028 Add validation stage ordering constants in src/Sigil.Sdk/Validation/ValidationPipeline.cs"
Task: "T030 Register schema validator as singleton in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs"

### US3

Task: "T032 Register validator and registries in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs"
Task: "T033 Fail fast on duplicate registry entries in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs"

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate deterministic, no-throw behavior for validation failures

### Incremental Delivery

1. Add US2 for schema-first enforcement and schema singleton initialization
2. Add US3 for registry-based offline routing and immutability
