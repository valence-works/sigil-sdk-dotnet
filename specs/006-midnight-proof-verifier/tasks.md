# Tasks: Midnight Proof System Verifier

**Input**: Design documents from `/specs/006-midnight-proof-verifier/`  
**Prerequisites**: `spec.md` (available), `plan.md` (not yet generated for this feature), repository baseline from `specs/005-proof-system-verifier-contract/plan.md`  
**Available Docs**: `spec.md`, `checklists/requirements.md`

**Tests**: Included. Spec 006 explicitly requires conformance and performance-oriented validation tasks.

**Organization**: Tasks are grouped by user story to preserve independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`) only for story phases
- All tasks include concrete file paths

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare conformance assets and test harness scaffolding for Midnight verification.

- [x] T001 Create conformance suite scaffold and vector contract documentation in tests/Sigil.Sdk.Tests/Validation/Conformance/README.md
- [x] T002 [P] Create known-valid vector placeholder for `urn:sigil:statement:license:v1` in tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-valid.json
- [x] T003 [P] Create known-invalid vector placeholder in tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-invalid.json
- [x] T004 [P] Create simulated internal-error vector placeholder in tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-internal-error.json
- [x] T005 Add conformance vector loader/model utilities in tests/Sigil.Sdk.Tests/Validation/Conformance/MidnightConformanceVectors.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core verifier plumbing and deterministic mapping that all stories depend on.

**✅ COMPLETE**: All foundational tasks finished.

- [x] T006 Confirm canonical proof-system identifier usage (`midnight-zk-v1`) in src/Sigil.Sdk/Proof/ProofSystemIds.cs
- [x] T007 Align proof verifier abstraction contracts for deterministic outcomes in src/Sigil.Sdk/Proof/IProofSystemVerifier.cs
- [x] T008 Align proof verification result contract for `Verified`/`Not Verified` semantics in src/Sigil.Sdk/Proof/ProofVerificationOutcome.cs
- [x] T009 Enforce deterministic verifier fault mapping (`Error`) in validation pipeline orchestration in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T010 Ensure default DI registration wires Midnight verifier via options in src/Sigil.Sdk/Validation/ValidationOptions.cs and src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs
- [x] T011 Add foundational registration and routing tests for Midnight verifier resolution in tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs

**Checkpoint**: Foundation complete; user stories can proceed.

---

## Phase 2.5: Proof of Concept - Wasmtime.NET WASM Bridge Integration

**Purpose**: Validate Wasmtime.NET integration strategy, extract WASM binary, establish PoC for latency measurement and function signature discovery.

**Context**: Q6 research concluded Wasmtime.NET is the optimal WASM bridge strategy. Phase 2.5 validates this decision with working PoC before Phase 3 full implementation.

**Timeline**: 1-2 weeks. Must complete before T014 implementation.

### Dependency & Binary Setup

- [x] T012 [P] Add Wasmtime.NET v16+ NuGet dependency to src/Sigil.Sdk/Sigil.Sdk.csproj
- [x] T013 [P] Extract `@midnight-ntwrk/proof-verification` WASM binary from npm package and create extraction script in .specify/scripts/extract-midnight-wasm.sh
- [x] T014 [P] Validate extracted WASM binary checksum against known-good hash and document in docs/
- [x] T015 Create WasmBinaryEmbedding test to verify WASM binary is properly embedded as EmbeddedResource in build

### PoC Implementation

- [x] T016 Build minimal PoC Wasmtime.NET instance loader in src/Sigil.Sdk/Proof/WasmtimeMidnightLoader.cs (private utility, not public API)
- [x] T017 Reverse-engineer WASM function signatures from `@midnight-ntwrk/proof-verification` TypeScript definitions and document in docs/MIDNIGHT_WASM_SIGNATURES.md
- [x] T018 Implement single-proof verification demo in PoC implementation (minimal, not production-ready)
- [x] T019 Add timeout and exception handling to PoC for graceful WASM fault modes

### Latency Validation

- [x] T020 Create latency micro-benchmark in tests/Sigil.Sdk.Tests/Performance/ using BenchmarkDotNet on 10+ known-valid proofs
- [x] T021 Document baseline latencies: module load, cold verification, warm verification, marshalling overhead in tests/Performance/MIDNIGHT_LATENCY_BASELINE.txt
- [x] T022 Validate latency compliance: confirm all measurements <120ms per proof (fits SC-004 budget)

### Test Vector Research & Acquisition

- [x] T023 **Acquire** (not just research) Midnight conformance test vectors meeting FR-015 requirements: minimum 3 vectors (valid, invalid, internal-error) from community or generated via documented process in docs/MIDNIGHT_TEST_VECTOR_ACQUISITION.md
  > Success criteria: 3 vectors obtained with documented source/checksums; vectors parseable into JSON structure
- [x] T024 [P] Create placeholder test vector structure in tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/ pending vector acquisition

**Checkpoint**: PoC validated. WASM bridge strategy confirmed. Ready for Phase 3 production implementation.

---

## Phase 3: User Story 1 - Verify Midnight proofs in validation pipeline (Priority: P1) 🎯 MVP

**Goal**: Deterministic, offline Midnight verification for valid/invalid proofs with correct `Invalid` mapping.

**Independent Test**: Run conformance vectors (known-valid + known-invalid) and verify deterministic outcomes for repeated runs.

### Tests for User Story 1

- [x] T025 [US1] Create Midnight conformance test harness for vector-driven validation in tests/Sigil.Sdk.Tests/Validation/MidnightProofConformanceTests.cs
- [x] T026 [US1] Add known-valid and known-invalid vector assertions for `license:v1` in tests/Sigil.Sdk.Tests/Validation/MidnightProofConformanceTests.cs
- [x] T026a [US1] Add test assertions verifying Midnight verifier validates proof cryptography only (FR-008: does NOT interpret statement claims or business semantics); add explicit negative tests for claim-free verification in tests/Sigil.Sdk.Tests/Validation/MidnightProofConformanceTests.cs

### Implementation for User Story 1

- [x] T027 [US1] Implement offline Midnight proof verification path in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
  > ✅ **UNBLOCKED**: Q6 research complete. Use Wasmtime.NET bridge per Phase 2.5 PoC. See [research.md](research.md#q6-which-midnight-libraries-provide-zk-verification-and-how-should-wasm-components-be-bridged-to-c) Q6 and [plan.md](plan.md#phase-0-research-resolution-midnight-library--wasm-bridge-strategy).
- [x] T028 [US1] Enforce statement-context compatibility gate for `urn:sigil:statement:license:v1` in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T029 [US1] Map Midnight cryptographic verification failure deterministically to `Invalid` in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T030 [US1] Ensure proof verification context construction remains statement-bound without claim interpretation in src/Sigil.Sdk/Proof/ProofVerificationContext.cs and src/Sigil.Sdk/Validation/LicenseValidator.cs

**Checkpoint**: US1 is independently functional and testable (MVP).

---

## Phase 4: User Story 2 - Diagnose failures safely without leaking proof material (Priority: P2)

**Goal**: Deterministic `Invalid` vs `Error` behavior with strict diagnostics redaction.

**Independent Test**: Force invalid-proof and simulated internal-fault paths and verify status mapping plus zero `proofBytes` leakage.

### Tests for User Story 2

- [x] T031 [US2] Add simulated internal-error vector assertions for deterministic `Error` mapping in tests/Sigil.Sdk.Tests/Validation/MidnightProofConformanceTests.cs
- [x] T032 [US2] Add proof-material redaction tests for diagnostics/logging in tests/Sigil.Sdk.Tests/Logging/ValidationLoggingTests.cs

### Implementation for User Story 2

- [x] T033 [US2] Implement safe verifier exception handling with redacted outward errors in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T034 [US2] Enforce no-`proofBytes` emission in validator diagnostics and failure reporting in src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T035 [US2] Ensure deterministic internal verifier failure code path (`ProofVerifierInternalError`) is mapped to `Error` in src/Sigil.Sdk/Validation/LicenseFailureCode.cs and src/Sigil.Sdk/Validation/LicenseValidator.cs
- [x] T036 [US2] Align failure classification metadata for Midnight verifier failures in src/Sigil.Sdk/Validation/FailureClassification.cs

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Reuse initialized Midnight verifier state safely (Priority: P3)

**Goal**: Deterministic startup initialization and safe verifier-state reuse without dominating latency budget.

**Independent Test**: Validate repeated and concurrent requests in one process and verify stable outcomes plus SC-004 performance bound.

### Tests for User Story 3

- [x] T037 [US3] Add initialization and reuse determinism tests across repeated **and concurrent** validations (explicit thread-safety concurrency harness: validate N parallel verifications produce deterministic outcomes without race conditions) in tests/Sigil.Sdk.Tests/Validation/MidnightVerifierInitializationTests.cs
  > Success criteria: Concurrent throughput measured; zero race condition failures; performance ≥ baseline sequential
- [x] T038 [US3] Add Midnight budget-share benchmark assertions aligned to Spec 002 SC-004 method in tests/Sigil.Sdk.Tests/Performance/ValidationPerformanceBenchmarks.cs

### Implementation for User Story 3

- [x] T039 [US3] Implement one-time Midnight verifier initialization lifecycle in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T040 [US3] Implement thread-safe reuse of initialized verifier state for concurrent validation (FR-010: safe reuse without changing outcomes for identical inputs) in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs
- [x] T041 [US3] Add verification-stage timing capture for budget accounting in src/Sigil.Sdk/Validation/LicenseValidator.cs

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cross-story documentation, traceability, and final execution validation.

- [x] T042 [P] Document Midnight verifier behavior, boundaries, and fail-closed guarantees in docs/architecture.md
- [x] T043 [P] Add Midnight verifier usage and diagnostics guidance in README.md
- [x] T044 Create Spec 006 execution scenarios and expected outcomes in specs/006-midnight-proof-verifier/quickstart.md
- [x] T045 Update feature execution plan notes and phase checkpoints in specs/006-midnight-proof-verifier/plan.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all story work.
- **Phase 2.5 (PoC)**: Depends on Phase 2; validates WASM bridge before production implementation.
- **Phase 3+ (User Stories)**: Depends on Phase 2.5 completion.
- **Phase 6 (Polish)**: Depends on completion of targeted stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2.5; no dependency on US2/US3.
- **US2 (P2)**: Starts after Phase 2.5; relies on US1 verifier execution path for error/diagnostics behavior.
- **US3 (P3)**: Starts after Phase 2.5; relies on US1 baseline verifier flow for initialization/reuse optimization.

### Story Completion Order (Dependency Graph)

`US1 -> US2 -> US3`

---

## Parallel Opportunities

### Phase 1 (Setup)

- Run `T002`, `T003`, and `T004` in parallel (separate vector files).

### Phase 2.5 (PoC)

- Run `T012`-`T015` in parallel (dependency & binary setup, independent files)
- Run `T023` and `T024` in parallel (test vector research, independent research tasks)

### User Story 1

- No safe parallel file-split inside US1 after `T025` because `T026`-`T030` mostly converge on verifier and validator core files.

### User Story 2

- Run `T032` and `T033` in parallel (logging tests vs verifier implementation files).

### User Story 3

- Run `T037` and `T038` in parallel (validation tests vs performance benchmark file).

---

## Parallel Execution Examples

### Phase 2.5 Dependency & Binary Setup

- Task T012: Add Wasmtime.NET v16+ NuGet dependency to src/Sigil.Sdk/Sigil.Sdk.csproj
- Task T013: Extract `@midnight-ntwrk/proof-verification` WASM binary from npm package
- Task T014: Validate extracted WASM binary checksum
- Task T015: Create WasmBinaryEmbedding test

### User Story 2

- Task T032: Add proof-material redaction tests for diagnostics/logging in tests/Sigil.Sdk.Tests/Logging/ValidationLoggingTests.cs
- Task T033: Implement safe verifier exception handling with redacted outward errors in src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs

### User Story 3

- Task T037: Add initialization and reuse determinism tests in tests/Sigil.Sdk.Tests/Validation/MidnightVerifierInitializationTests.cs
- Task T038: Add Midnight budget-share benchmark assertions in tests/Sigil.Sdk.Tests/Performance/ValidationPerformanceBenchmarks.cs

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 2.5 (PoC) with working Wasmtime.NET integration.
3. Complete Phase 3 (US1).
4. Validate US1 independently with conformance vectors.
5. Demo/deploy MVP behavior for `midnight-zk-v1`.

### Incremental Delivery

1. Setup & Foundational (Phases 1-2)
2. Proof of Concept (Phase 2.5) - validate bridge before production
3. US1 (MVP - core verification)
4. US2 (diagnostics + deterministic fault mapping)
5. US3 (initialization/reuse + performance)
6. Polish and documentation

### Team Parallelization

1. One developer handles Phase 2.5 (WASM bridge PoC)
2. One developer handles verifier core (Phase 3+: `src/Sigil.Sdk/Proof/*`)
3. One developer handles validator mapping/logging (`src/Sigil.Sdk/Validation/*`)
4. One developer handles conformance/performance tests (`tests/Sigil.Sdk.Tests/*`)

---

## Task Summary

| Phase | ID Range | Count | Purpose |
|-------|----------|-------|---------|
| Phase 1 | T001-T011 | 11 | Setup conformance scaffolding |
| Phase 2 | (T001-T011 overlap) | 6 | Foundational verifier plumbing |
| **Phase 2.5** | **T012-T024** | **13** | **Wasmtime.NET WASM bridge PoC** |
| Phase 3 (US1) | T025-T030 | 6 | Core Midnight verification |
| Phase 4 (US2) | T031-T036 | 6 | Diagnostics + safe failure handling |
| Phase 5 (US3) | T037-T041 | 5 | Initialization + thread-safe reuse |
| Phase 6 | T042-T045 | 4 | Polish & documentation |
| **TOTAL** | **T001-T045** | **45** | **Complete Spec 006 implementation** |

---

## Notes

- Task IDs are sequential and executable.
- `[P]` is used where tasks can safely run in parallel (different files, no incomplete-task dependency).
- Story labels (`[US1]`, `[US2]`, `[US3]`) are only applied in user story phases (3-5).
- Phase 2.5 (PoC) is critical path blocker for T027+ (production verifier implementation).
- Tests are included throughout because Spec 006 explicitly requires conformance and measurable outcomes.
