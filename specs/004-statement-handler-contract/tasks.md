# Tasks: Statement Handler Contract & license:v1 Statement Definition

**Input**: Design documents from `/specs/004-statement-handler-contract/`
**Prerequisites**: `plan.md` (required), `spec.md` (required), `research.md`, `data-model.md`, `contracts/`, `quickstart.md`
**Tests**: No explicit TDD requirement in spec; tasks focus on implementation and verification.
**Organization**: Tasks are grouped by user story to enable independent implementation and validation.

## Format: `- [ ] T### [P?] [US?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[US#]**: User story label required for story-phase tasks
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align feature baseline and prepare project files for Spec 004 implementation.

- [ ] T001 Confirm .NET 10 build baseline and feature scope notes in specs/004-statement-handler-contract/plan.md (supports constraints + SC-007)
- [ ] T002 Align feature quickstart runtime notes in specs/004-statement-handler-contract/quickstart.md (supports SC-007)
- [ ] T003 [P] Verify statement contract artifacts exist and are current in specs/004-statement-handler-contract/contracts/statement-handler-contract.md (supports FR-001, FR-005)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core statement-contract infrastructure required before user story implementation.

**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [ ] T004 Add canonical statement identifiers in src/Sigil.Sdk/Statements/StatementIds.cs
- [ ] T005 Update statement handler contract shape (`StatementId`, result invariants, XML docs) in src/Sigil.Sdk/Statements/IStatementHandler.cs
- [ ] T006 [P] Update statement registry API docs and lookup semantics in src/Sigil.Sdk/Registries/IStatementRegistry.cs
- [ ] T007 Update duplicate detection and deterministic keying by statement ID in src/Sigil.Sdk/Validation/ValidationOptions.cs
- [ ] T008 Update statement registry construction to use handler statement IDs in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs
- [ ] T009 Harden duplicate-key and null-handler guards in src/Sigil.Sdk/Registries/ImmutableStatementRegistry.cs
- [ ] T010 Add/confirm deterministic failure mapping for statement contract violations in src/Sigil.Sdk/Validation/FailureClassification.cs

**Checkpoint**: Foundation ready — user stories can now proceed.

---

## Phase 3: User Story 1 — Understand statement concept and handler contract (Priority: P1) 🎯 MVP

**Goal**: Formalize extensibility contract in code and docs so custom handlers are unambiguous.
**Independent Test**: Implement a minimal custom handler and verify contract expectations (cancellation, deterministic invalid result, non-null claims on success) are clearly documented and enforced.

### Implementation for User Story 1

- [ ] T011 [US1] Enforce `IsValid=true` requires non-null claims in validation pipeline in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [ ] T012 [US1] Ensure cooperative cancellation behavior is preserved for statement handlers in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [ ] T013 [P] [US1] Clarify statement/validator responsibility boundaries in docs/architecture.md
- [ ] T014 [P] [US1] Document custom statement handler contract and error expectations in docs/DI_INTEGRATION.md
- [ ] T015 [US1] Align feature contract wording with implementation behavior in specs/004-statement-handler-contract/contracts/statement-handler-contract.md

**Checkpoint**: Statement extensibility contract is explicit and enforceable.

---

## Phase 4: User Story 2 — Implement built-in license:v1 statement handler (Priority: P2)

**Goal**: Provide strict `license:v1` semantic validation and deterministic claim extraction.
**Independent Test**: Validate `license:v1` inputs with required/optional fields, malformed values, and unknown fields; verify deterministic pass/fail and extracted claims.

### Implementation for User Story 2

- [ ] T016 [US2] Implement built-in `license:v1` handler with strict field validation in src/Sigil.Sdk/Statements/LicenseV1StatementHandler.cs
- [ ] T017 [P] [US2] Add helper validation for `features` kebab-case and duplicate detection in src/Sigil.Sdk/Statements/LicenseV1StatementHandler.cs
- [ ] T018 [US2] Implement typed claim model fields and constructors for `productId`, `edition`, `features`, `expiresAt`, `maxSeats`, `issuedAt` in src/Sigil.Sdk/Validation/LicenseClaims.cs
- [ ] T019 [US2] Register built-in `license:v1` statement handler by default in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs
- [ ] T020 [US2] Ensure statement resolution path uses canonical statement IDs in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [ ] T021 [P] [US2] Align statement definition contract with final field/type rules in specs/004-statement-handler-contract/contracts/license-v1-statement.md

**Checkpoint**: `license:v1` statement handler is fully functional and deterministic.

---

## Phase 5: User Story 3 — Clarify expiry evaluation as statement semantics (Priority: P3)

**Goal**: Keep conceptual ownership in statement semantics while executing expiry in validator stage.
**Independent Test**: Verify expiry runs only after successful statement validation and uses validated claim values, returning deterministic `Expired` outcomes.

### Implementation for User Story 3

- [ ] T022 [US3] Refactor expiry stage to consume validated claims instead of raw JSON in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [ ] T023 [US3] Normalize `expiresAt` handling to statement-defined type semantics in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [ ] T024 [P] [US3] Clarify pipeline-stage constants/comments for statement vs expiry stages in src/Sigil.Sdk/Validation/ValidationPipeline.cs
- [ ] T025 [P] [US3] Align expiry semantic contract with implemented pipeline ordering in specs/004-statement-handler-contract/contracts/expiry-evaluation.md
- [ ] T026 [US3] Update architecture narrative for conceptual ownership vs execution stage in docs/architecture.md

**Checkpoint**: Expiry semantics are clear, deterministic, and pipeline-consistent.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency, documentation polish, and verification across all stories.

- [ ] T027 [P] Synchronize feature quickstart with implemented API and field semantics in specs/004-statement-handler-contract/quickstart.md (supports SC-007)
- [ ] T028 [P] Update feature data model to match final implemented claim/expiry behavior in specs/004-statement-handler-contract/data-model.md (supports FR-010, FR-012)
- [ ] T029 Perform end-to-end feature consistency review across spec/plan/contracts in specs/004-statement-handler-contract/spec.md (supports FR-001, FR-006)
- [ ] T030 Run focused validation-related test suite in tests/Sigil.Sdk.Tests/Sigil.Sdk.Tests.csproj (supports SC-002, SC-003)
- [ ] T031 [P] Document handler determinism and logging guidance (no full `publicInputs` logging) in docs/DI_INTEGRATION.md
- [ ] T032 Add deterministic expiry failure mapping verification in src/Sigil.Sdk/Validation/FailureClassification.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user stories
- **User Stories (Phase 3–5)**: Depend on Foundational completion
- **Polish (Phase 6)**: Depends on completion of targeted user stories

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only; MVP slice
- **US2 (P2)**: Depends on Foundational; can proceed after US1 contract baseline
- **US3 (P3)**: Depends on US2 claim model and statement semantics alignment

### Dependency Graph

- Setup → Foundational → US1 → US2 → US3 → Polish
- US1 provides contract baseline that US2 and US3 build on

---

## Parallel Execution Examples

### US1

- T013 and T014 can run in parallel (different documentation files)

### US2

- T017 and T021 can run in parallel (implementation helper vs contract documentation)

### US3

- T024 and T025 can run in parallel (pipeline constants vs contract documentation)

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2
2. Complete Phase 3 (US1)
3. Validate statement contract clarity and enforcement
4. Demo custom-handler extensibility baseline

### Incremental Delivery

1. Deliver US1 contract baseline
2. Deliver US2 built-in `license:v1` behavior
3. Deliver US3 expiry semantic alignment
4. Finish Phase 6 cross-cutting consistency and verification

### Team Parallelization

1. One engineer executes core runtime tasks (`LicenseValidator`, `ValidationOptions`, registries)
2. One engineer executes statement-specific implementation (`LicenseV1StatementHandler`, `LicenseClaims`)
3. One engineer executes contract/docs alignment tasks (`contracts/`, `quickstart`, `architecture`)
