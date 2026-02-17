# Tasks: SDK Validation API

**Input**: Design documents from /specs/002-sdk-validation-api/

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: The spec includes measurable success criteria; add minimal verification tests to demonstrate deterministic results, no-throw behavior for validation failures, and proofBytes redaction.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: - [ ] T### [P?] [US?] Description with file path

- **[P]**: Can run in parallel (different files, no dependencies)
- **[US#]**: Which user story this task belongs to
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the minimal .NET SDK project structure to implement Spec 002.

- [x] T001 Create solution file at Sigil.Sdk.sln
- [x] T002 Create SDK project at src/Sigil.Sdk/Sigil.Sdk.csproj (net8.0 class library; include required package references)
- [x] T003 Create test project skeleton at tests/Sigil.Sdk.Tests/Sigil.Sdk.Tests.csproj (no tests required yet)
- [x] T004 [P] Add repo-wide .editorconfig at .editorconfig
- [x] T005 [P] Add repo-wide Directory.Build.props for common .NET settings at Directory.Build.props

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core building blocks required by all user stories.

**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [x] T006 Add core status enum in src/Sigil.Sdk/Validation/LicenseStatus.cs
- [x] T007 Add deterministic failure code enum with explicit numeric values in src/Sigil.Sdk/Validation/LicenseFailureCode.cs
- [x] T008 [P] Add failure model in src/Sigil.Sdk/Validation/LicenseValidationFailure.cs
- [x] T009 [P] Add result model in src/Sigil.Sdk/Validation/LicenseValidationResult.cs
- [x] T010 [P] Add claims placeholder model in src/Sigil.Sdk/Validation/LicenseClaims.cs
- [x] T011 Add validation options (including diagnostics opt-in) in src/Sigil.Sdk/Validation/ValidationOptions.cs
- [x] T012 Add clock abstraction in src/Sigil.Sdk/Time/IClock.cs and default implementation in src/Sigil.Sdk/Time/SystemClock.cs
- [x] T013 Add logging policy helpers in src/Sigil.Sdk/Logging/ValidationLogging.cs (must never accept/log proofBytes)
- [x] T014 Add schema file copy for runtime use at src/Sigil.Sdk/Contracts/proof-envelope.schema.json (source from specs/001-proof-envelope-format/contracts/proof-envelope.schema.json)
- [x] T015 Add schema validator abstraction in src/Sigil.Sdk/Schema/IProofEnvelopeSchemaValidator.cs
- [x] T016 Implement Draft 2020-12 schema validation using Corvus.Json.Validator in src/Sigil.Sdk/Schema/ProofEnvelopeSchemaValidator.cs (schema initialized once and reused)
- [x] T017 Add registry + handler contracts in src/Sigil.Sdk/Registries/IProofSystemRegistry.cs, src/Sigil.Sdk/Registries/IStatementRegistry.cs, src/Sigil.Sdk/Proof/IProofSystemVerifier.cs, and src/Sigil.Sdk/Statements/IStatementHandler.cs
- [x] T018 Implement immutable registries in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs and src/Sigil.Sdk/Registries/ImmutableStatementRegistry.cs
- [x] T019 Add envelope parsing/early extraction in src/Sigil.Sdk/Envelope/ProofEnvelopeReader.cs (extract envelopeVersion, proofSystem, statementId before schema/crypto)

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Validate a proof envelope deterministically (Priority: P1) 🎯 MVP

**Goal**: Provide a result-object `ValidateAsync` API returning deterministic `LicenseStatus` + exactly one failure code for all non-`Valid` outcomes.

**Independent Test**: Call `ValidateAsync(string)` twice with the same invalid input; results match (same status + code) and no exception is thrown (except programmer errors like null arguments).

- [x] T020 [P] [US1] Define validator interface in src/Sigil.Sdk/Validation/ILicenseValidator.cs
- [x] T021 [US1] Implement ValidateAsync(string, CancellationToken) in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T022 [US1] Implement ValidateAsync(Stream, CancellationToken) in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T023 [P] [US1] Add deterministic first-failing-stage mapping helpers in src/Sigil.Sdk/Validation/FailureClassification.cs
- [x] T024 [US1] Enforce exactly-one-failure-code invariant in src/Sigil.Sdk/Validation/LicenseValidationResult.cs
- [x] T025 [US1] Ensure null inputs throw ArgumentNullException in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T026 [US1] Implement post-verification expiresAt evaluation in src/Sigil.Sdk/Validation/LicenseValidator.cs (FR-008, FR-008a, FR-008b)
- [x] T027 [US1] Add deterministic expiry-related failure codes in src/Sigil.Sdk/Validation/LicenseFailureCode.cs and map them in src/Sigil.Sdk/Validation/FailureClassification.cs
- [x] T028 [US1] Add deterministic stream-read failure classification (IO error/truncation/disposal during read) in src/Sigil.Sdk/Validation/LicenseValidator.cs (FR-017a)
- [x] T029 [US1] Add deterministic result and no-throw verification tests in tests/Sigil.Sdk.Tests/Validation/LicenseValidatorTests.cs (SC-001, SC-002)
- [x] T030 [US1] Add proofBytes redaction verification tests in tests/Sigil.Sdk.Tests/Logging/ValidationLoggingTests.cs (SC-005)
- [x] T031 [US1] Implement cryptographic verification stage in src/Sigil.Sdk/Validation/LicenseValidator.cs (invoke IProofSystemVerifier resolved from registry; crypto failure returns Invalid per FR-016)
- [x] T032 [US1] Implement statement semantic validation stage in src/Sigil.Sdk/Validation/LicenseValidator.cs (invoke IStatementHandler resolved from registry; semantic failures return Invalid per FR-016)
- [x] T033 [US1] Add crypto-invalid and expiry-after-verify tests in tests/Sigil.Sdk.Tests/Validation/LicenseValidatorCryptoTests.cs (FR-016, FR-008a, FR-008b, SC-003)

**Checkpoint**: Callers can validate input and always receive deterministic result objects.

---

## Phase 4: User Story 2 — Enforce schema-first validation (Priority: P2)

**Goal**: Guarantee schema validation runs before any cryptographic verification, and schema compilation occurs once at startup.

**Independent Test**: Provide schema-invalid input and confirm the crypto verifier is never invoked (result is `Malformed`).

- [x] T034 [US2] Add validation pipeline stage ordering constants in src/Sigil.Sdk/Validation/ValidationPipeline.cs
- [x] T035 [US2] Implement schema-first gating in src/Sigil.Sdk/Validation/LicenseValidator.cs (schema failures return Malformed)
- [x] T036 [US2] Ensure schema initialization is singleton/startup-only via DI in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs

**Checkpoint**: Schema is validated before crypto on every path.

---

## Phase 5: User Story 3 — Support offline registry resolution (Priority: P3)

**Goal**: Resolve `proofSystem` and `statementId` via immutable DI registries, and return deterministic `Unsupported` failures for unknown identifiers.

**Independent Test**: With empty registries, validate a schema-valid envelope and confirm status is `Unsupported` with deterministic failure code.

- [x] T037 [US3] Add registry resolution stage in src/Sigil.Sdk/Validation/LicenseValidator.cs (unknown proofSystem/statementId → Unsupported)
- [x] T038 [US3] Add DI registration entry point in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs (register validator, schema validator, clock, registries)
- [x] T039 [US3] Enforce registry immutability and duplicate-key detection at startup in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs and src/Sigil.Sdk/Registries/ImmutableStatementRegistry.cs

**Checkpoint**: Registry-driven routing is deterministic and offline.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cross-cutting hardening aligned with constitution gates.

- [x] T040 [P] Document failure-code mapping table in specs/002-sdk-validation-api/contracts/failure-codes.md
- [x] T041 [P] Validate logging rules are captured in specs/002-sdk-validation-api/contracts/logging-policy.md
- [x] T042 Ensure fail-closed behavior for unexpected exceptions in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T043 Run and validate quickstart scenarios in specs/002-sdk-validation-api/quickstart.md
- [x] T044 Add SC-004 performance verification benchmark in tests/Sigil.Sdk.Tests/Performance/ValidationPerformanceBenchmarks.cs (use the SC-004 measurement method from spec.md)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup
- **User Stories (Phase 3–5)**: all depend on Foundational
- **Polish (Phase 6)**: depends on Phases 3–5 being complete

### User Story Dependencies

- **US1 (P1)**: depends on Phase 2 only
- **US2 (P2)**: depends on US1 (extends the US1 pipeline with explicit schema-first gating + startup schema initialization)
- **US3 (P3)**: depends on US1 (adds immutable DI registries + deterministic Unsupported behavior)

---

## Parallel Execution Examples

### US1

- Parallel candidates:
  - `T020` (interface) and `T023` (classification helpers)

### US2

- Parallel candidates:
  - `T034` (pipeline constants) can run alongside `T036` (DI singleton schema init)

### US3

- Parallel candidates:
  - `T030` (DI registration) and `T031` (registry immutability enforcement)

---

## Implementation Strategy

### MVP Scope (US1)

1. Complete Phases 1–2
2. Complete Phase 3 (US1)
3. Validate determinism + no-throw behavior for validation failures

### Incremental Delivery

- Add US2 to harden schema-first gating and startup schema compilation
- Add US3 to harden registry-based offline routing and immutability
