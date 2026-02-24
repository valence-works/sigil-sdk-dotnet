# Tasks: Proof System Verifier Contract

**Input**: Design documents from `/specs/005-proof-system-verifier-contract/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/proof-system-verifier.openapi.yaml`

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare project files and baseline documentation for Spec 005 implementation.

- [x] T001 Create initial quickstart scaffold sections for implementation steps in specs/005-proof-system-verifier-contract/quickstart.md
- [x] T002 Add Spec 005 implementation checklist anchors in specs/005-proof-system-verifier-contract/plan.md
- [x] T003 [P] Add Spec 005 implementation section to README.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and pipeline hooks required before user-story work.

**⚠️ CRITICAL**: No user story implementation should start before this phase is complete.

- [x] T004 Define proof verification context model in src/Sigil.Sdk/Proof/ProofVerificationContext.cs
- [x] T005 Define proof verification outcome model in src/Sigil.Sdk/Proof/ProofVerificationOutcome.cs
- [x] T006 Update verifier contract signature and docs in src/Sigil.Sdk/Proof/IProofSystemVerifier.cs
- [x] T007 [P] Add deterministic proof-stage failure code(s) in src/Sigil.Sdk/Validation/LicenseFailureCode.cs
- [x] T008 [P] Add validation pipeline stage constants for verifier mapping in src/Sigil.Sdk/Validation/ValidationPipeline.cs
- [x] T009 Wire foundational verifier result handling in src/Sigil.Sdk/Validation/LicenseValidator.cs

**Checkpoint**: Verifier abstractions and pipeline primitives are in place.

---

## Phase 3: User Story 1 - Verify supported proofs deterministically (Priority: P1) 🎯 MVP

**Goal**: Deliver deterministic, offline proof verification for supported proof systems with initial Midnight support.

**Independent Test**: Same `proofBytes` + context + registry config returns identical status/failure outputs across repeated validations.

### Implementation for User Story 1

- [x] T010 [US1] Implement canonical `midnight-zk-v1` verifier in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T011 [US1] Register default `midnight-zk-v1` verifier during setup in src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs
- [x] T012 [US1] Implement deterministic proof verification mapping for supported systems in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T013 [US1] Enforce offline-only verifier execution path in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T014 [P] [US1] Add deterministic verifier result type usage in src/Sigil.Sdk/Proof/ProofVerificationOutcome.cs
- [x] T015 [P] [US1] Add Midnight verifier configuration entry points in src/Sigil.Sdk/Validation/ValidationOptions.cs

**Checkpoint**: Supported proof verification works deterministically and offline for Midnight-first path.

---

## Phase 4: User Story 2 - Resolve proof systems through immutable DI registries (Priority: P2)

**Goal**: Ensure canonical proof-system resolution via immutable registry with deterministic unsupported behavior.

**Independent Test**: Unknown/unregistered or case-variant `proofSystem` fails with deterministic `Unsupported`, and duplicates fail at registration/build.

### Implementation for User Story 2

- [x] T016 [US2] Enforce case-sensitive canonical key resolution in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs
- [x] T017 [US2] Enforce one-verifier-per-proofSystem guard in src/Sigil.Sdk/Validation/ValidationOptions.cs
- [x] T018 [US2] Ensure unsupported proof-system mapping remains deterministic in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T019 [P] [US2] Add immutable registry mutation guard behavior in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs
- [x] T020 [P] [US2] Align proof-system contract examples with canonical lookup behavior in specs/005-proof-system-verifier-contract/contracts/proof-system-verifier.openapi.yaml

**Checkpoint**: Proof-system resolution is immutable, canonical, and deterministic.

---

## Phase 5: User Story 3 - Preserve fail-closed semantics and safe diagnostics (Priority: P3)

**Goal**: Preserve first-failing-stage behavior, deterministic status mapping, and proof-material confidentiality.

**Independent Test**: Schema/unsupported failures remain dominant, verifier internal faults map to `Error`, cancellation propagates, and `proofBytes` never appears in diagnostics.

### Implementation for User Story 3

- [x] T021 [US3] Implement first-failing-stage precedence guards for proof stage in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T022 [US3] Implement verifier internal-error and incompatibility mapping rules in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T023 [US3] Implement cancellation propagation (no status remap) in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T024 [P] [US3] Add redaction-safe diagnostic logging updates in src/Sigil.Sdk/Logging/ValidationLogging.cs
- [x] T025 [P] [US3] Align validator diagnostics configuration defaults with redaction policy in src/Sigil.Sdk/Validation/ValidationOptions.cs

**Checkpoint**: Fail-closed pipeline and proof confidentiality guarantees are enforced.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation and non-functional validation across stories.

- [x] T026 [P] Align verifier contract documentation with finalized behavior in specs/005-proof-system-verifier-contract/contracts/proof-system-verifier.openapi.yaml
- [x] T027 [P] Record final behavior and extension notes in docs/architecture.md
- [x] T028 Publish final conformance outcomes and evidence links in specs/005-proof-system-verifier-contract/quickstart.md
- [x] T029 [P] Execute deterministic replay conformance matrix and record results in specs/005-proof-system-verifier-contract/quickstart.md
- [x] T030 [P] Execute diagnostics redaction audit and record results in specs/005-proof-system-verifier-contract/quickstart.md
- [x] T031 Execute p95 benchmark run for <=10 KB envelopes and record measurement method/results in specs/005-proof-system-verifier-contract/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: Depends on Phase 2; can run in parallel with US1 after foundation but should merge after T012 for reduced conflict risk.
- **Phase 5 (US3)**: Depends on Phase 2; should follow core mapping work in US1/US2 to avoid rework.
- **Phase 6 (Polish)**: Depends on completion of selected user stories.

### User Story Dependencies

- **US1 (P1)**: Independent after Foundational completion.
- **US2 (P2)**: Independent after Foundational completion; integrates with registry and validator touched in US1.
- **US3 (P3)**: Independent after Foundational completion; finalizes pipeline semantics and diagnostics behavior.

### Within Each User Story

- Implement core contracts/mapping first.
- Add conformance coverage after behavior is implemented.
- Confirm each story meets its independent test criteria before moving on.

---

## Parallel Opportunities

- **Setup**: `T003` can run while `T001-T002` are in progress.
- **Foundational**: `T007` and `T008` can run in parallel once `T004-T006` are started.
- **US1**: `T014` and `T015` can run in parallel after `T010-T013`.
- **US2**: `T019` and `T020` can run in parallel after `T016-T018`.
- **US3**: `T024` and `T025` can run in parallel after `T021-T023`.
- **Polish**: `T026`, `T027`, `T029`, and `T030` can run in parallel before `T028` and `T031` finalization.

---

## Parallel Example: User Story 1

```bash
# After T010-T013 complete, run in parallel:
Task T014: Add deterministic verifier result type usage in src/Sigil.Sdk/Proof/ProofVerificationOutcome.cs
Task T015: Add Midnight verifier configuration entry points in src/Sigil.Sdk/Validation/ValidationOptions.cs
```

## Parallel Example: User Story 2

```bash
# After T016-T018 complete, run in parallel:
Task T019: Add immutable registry mutation guard behavior in src/Sigil.Sdk/Registries/ImmutableProofSystemRegistry.cs
Task T020: Align proof-system contract examples with canonical lookup behavior in specs/005-proof-system-verifier-contract/contracts/proof-system-verifier.openapi.yaml
```

## Parallel Example: User Story 3

```bash
# After T021-T023 complete, run in parallel:
Task T024: Add redaction-safe diagnostic logging updates in src/Sigil.Sdk/Logging/ValidationLogging.cs
Task T025: Align validator diagnostics configuration defaults with redaction policy in src/Sigil.Sdk/Validation/ValidationOptions.cs
```

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate deterministic and offline Midnight-first behavior.
4. Demo/release MVP slice.

### Incremental Delivery

1. Foundation complete -> begin story delivery.
2. Deliver US1 (deterministic supported verification).
3. Deliver US2 (immutable canonical resolution).
4. Deliver US3 (fail-closed precedence + redaction + cancellation semantics).
5. Finish with polish/documentation alignment and conformance evidence capture.

### Team Parallelization

1. One engineer completes foundational contract work.
2. Then split:
   - Engineer A: US1 verifier + default registration
   - Engineer B: US2 registry resolution behavior
   - Engineer C: US3 diagnostics/fail-closed semantics
3. Rejoin for Phase 6 documentation and quickstart validation.

---

## Notes

- All tasks follow strict checklist format: `- [ ] T### [P?] [US?] Description with file path`.
- `[US#]` labels are used only for user-story phases.
- Tasks are scoped for independent story validation and deterministic behavior guarantees.
