# Tasks: DI Integration & Defaults

**Input**: Design documents from `/specs/003-di-integration/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Constitution Checks (implemented in this plan)

- Registries registered via DI and immutable after container build ✓
- No `proofBytes` logging in diagnostics ✓
- Deterministic duplicate registration failures ✓
- Result-object validation API (existing in Spec 002) ✓
- Schema validation before proof verification (existing in Spec 002) ✓

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: DI extension method framework and supporting infrastructure

**Note on Sample Scope (Clarification A3)**: One minimal ASP.NET Core sample in `/samples/MinimalDiSample/` demonstrating 3-5 line basic setup. Console and worker service integration verified via tests (T048) and documented in guides (T051-T058); separate sample projects not required for release.

**Note on Sample Scope (Clarification A3)**: One minimal ASP.NET Core sample in `/samples/MinimalDiSample/` demonstrating 3-5 line basic setup. Console and worker service integration verified via tests (T048) and documented in guides (T051-T058); separate sample projects not required for release.

- [x] T001 Create `ServiceCollectionExtensions.AddSigilValidation()` extension method overloads in `src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T002 Implement duplicate-call guard in `AddSigilValidation()` to throw `InvalidOperationException` on second call
- [x] T003 Create complete `ValidationOptions` class in `src/Sigil.Sdk/Validation/ValidationOptions.cs` with:
  - `AddProofSystem(identifier, verifier)` builder method with duplicate detection
  - `AddStatementHandler(handler)` builder method with duplicate detection  
  - Logging configuration properties (`EnableDiagnostics`, `LogFailureDetails`, etc.)
  - XML documentation with examples for all public methods
- [x] T004 Create immutable registry construction logic in `src/Sigil.Sdk/Registries/` to build registries from ValidationOptions registrations
- [x] T005 Implement registry immutability enforcement in registry internals via one of these approaches:
  - Sealed/readonly wrapper class that prevents mutation after container build ✓ **IMPLEMENTED** 
  - Validation checks in all mutating methods (Add, Update, Remove) to throw if container is built
  - Interface-based design where runtime registries are read-only; only builder has mutable methods
  - Decision: **Sealed + IReadOnlyDictionary approach (Option 1)** - ImmutableProofSystemRegistry and ImmutableStatementRegistry use sealed class keyword and protect mutable collections with private readonly references.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core DI registration wiring and options handling

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 Register `ISigilValidator` as singleton in `AddSigilValidation()` with all dependencies resolved
- [x] T007 Register immutable `IProofSystemRegistry` as singleton with ValidationOptions registrations
- [x] T008 Register immutable `IStatementRegistry` as singleton with ValidationOptions registrations
- [x] T009 Register `IProofEnvelopeSchemaValidator` as singleton (compiled once at startup)
- [x] T010 Register `IClock` as singleton with `SystemClock` as default implementation
- [x] T011 Register `ValidationOptions` as singleton with production-ready defaults (diagnostics off, no sensitive logging)
- [x] T012 Add validation in `AddSigilValidation()` to throw `InvalidOperationException` if required dependencies are missing or misconfigured
- [x] T013 Update `ServiceCollectionExtensions.cs` to handle optional configuration delegate parameter with null safety

**Checkpoint**: Foundation ready - all core services registered and validated ✓ (32 unit tests passing)

---

## Phase 3: User Story 1 - Basic Validator Setup (Priority: P1) 🎯 MVP

**Goal**: Developers can call `services.AddSigilValidation()` and immediately use `ISigilValidator` with default configuration

**Independent Test**: Create minimal .NET app, register services with `AddSigilValidation()`, inject `ISigilValidator`, validate a proof envelope, receive result without errors

### Implementation for User Story 1

- [x] T014 [P] [US1] Create minimal sample app in `samples/MinimalDiSample/Program.cs` demonstrating 3-5 line setup
- [x] T015 [P] [US1] Create sample `Startup.cs` or `Program.cs` with `services.AddSigilValidation()` registration
- [x] T016 [P] [US1] Add controller or service in sample that injects and uses `ISigilValidator` for validation
- [x] T017 [US1] Update `src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs` with clear XML documentation for `AddSigilValidation()` with minimal setup example ✓ (comprehensive XML docs with examples)
- [x] T018 [US1] Update README or create `QUICKSTART.md` with 5-line integration example and link to sample ✓ (created samples/MinimalDiSample/README.md with full guide)
- [x] T019 [P] [US1] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying `AddSigilValidation()` registers all required services ✓ (32 tests passing)
- [x] T020 [P] [US1] Add unit test verifying `ISigilValidator` can be resolved from DI container after `AddSigilValidation()` ✓ (AddSigilValidation_RegistersISigilValidator)
- [x] T021 [P] [US1] Add unit test verifying validator executes successfully with default configuration ✓ (Integration tests in MinimalDiSample.Tests)
- [x] T022 [US1] Add integration test in sample project verifying end-to-end validation with valid and invalid envelopes ✓ (SampleApplicationIntegrationTests with 6 tests)

**Checkpoint**: User Story 1 complete - basic setup works with default configuration ✓

---

## Phase 4: User Story 2 - Custom Diagnostics Configuration (Priority: P2)

**Goal**: Developers can configure logging and diagnostics options during registration without affecting core validation behavior

**Independent Test**: Configure `options.EnableDiagnostics = false`, perform validation, verify no diagnostic output; configure `options.EnableDiagnostics = true`, perform validation, verify appropriate diagnostic logging

### Implementation for User Story 2

- [ ] T023 [P] [US2] Update `LicenseValidator` to respect `ValidationOptions` logging configuration when writing to logs
- [ ] T025 [P] [US2] Update `ValidationOptions` to accept and apply logging configuration options
- [ ] T025 [P] [US2] Add implementation in `AddSigilValidation()` to pass options to validator correctly
- [ ] T026 [P] [US2] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying configuration delegate is applied
- [ ] T027 [P] [US2] Add unit test verifying `options.LogFailureDetails = false` prevents detailed logging
- [ ] T028 [P] [US2] Add integration test verifying custom diagnostics configuration is respected during validation
- [ ] T029 [US2] Add sample code in `samples/MinimalDiSample/` demonstrating diagnostics configuration option
- [ ] T030 [US2] Update documentation with diagnostics configuration examples and best practices
- [ ] T031 [P] [US3] Implement `ValidationOptions.AddProofSystem(identifier, verifier)` method that adds verifier to internal collection and validates no duplicates
- [ ] T032 [P] [US3] Implement `ValidationOptions.AddStatementHandler(handler)` method that adds handler to internal collection

**Checkpoint**: User Stories 1 & 2 complete - diagnostics configuration working independently

---

## Phase 5: User Story 3 - Extension with Custom Systems (Priority: P3)

**Goal**: Developers can register custom proof system verifiers and statement handlers and have them integrated seamlessly into validation

**Independent Test**: Register custom proof system and statement handler via `options.AddProofSystem()` and `options.AddStatementHandler()`, validate envelope containing custom types, verify custom implementation is invoked

### Implementation for User Story 3

- [ ] T033 [P] [US3] Update registry construction in Phase 2 to include custom systems from `ValidationOptions`
- [ ] T034 [P] [US3] Add duplicate identifier detection in `AddProofSystem()` to throw `InvalidOperationException` immediately
- [ ] T035 [P] [US3] Add duplicate identifier detection in `AddStatementHandler()` to throw `InvalidOperationException` immediately
- [ ] T036 [P] [US3] Update XML documentation in `ValidationOptions` with clear examples for `AddProofSystem()` and `AddStatementHandler()`
- [ ] T037 [P] [US3] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying custom proof system can be registered
- [ ] T038 [P] [US3] Add unit test verifying duplicate proof system identifier throws `InvalidOperationException`
- [ ] T039 [P] [US3] Add unit test verifying custom statement handler can be registered
- [ ] T040 [P] [US3] Add unit test verifying duplicate statement identifier throws `InvalidOperationException`
- [ ] T041 [US3] Add integration test verifying custom verifier/handler is invoked during validation when applicable
- [ ] T042 [P] [US3] Add sample code in `samples/MinimalDiSample/` demonstrating custom system registration and extension usage
- [ ] T043 [US3] Update documentation with extension point examples and patterns for developers adding custom systems
- [ ] T044 [P] Add unit test verifying second call to `AddSigilValidation()` throws `InvalidOperationException` with clear message
- [ ] T045 [P] Add unit test verifying missing required dependencies throw `InvalidOperationException` during registration with guidance

**Checkpoint**: All user stories complete - extension points working and documented

---

## Phase 6: Edge Cases & Error Handling

**Purpose**: Implement and test all edge cases identified in spec

- [ ] T046 [P] Add unit test verifying exception from custom verifier during registration is wrapped as `InvalidOperationException`
- [ ] T047 [P] Add unit test verifying runtime modification of registries after container build throws `InvalidOperationException`
- [ ] T048 [P] Add integration test for multiple application hosting scenarios (ASP.NET Core, console app, worker service)
- [ ] T049 [P] Add performance test verifying schema compilation completes in under 100 milliseconds
- [ ] T050 [P] Add memory profiling test verifying service overhead under 5 MB with default configuration
- [ ] T051 Create comprehensive integration guide in `docs/DI_INTEGRATION.md` with minimal setup, options configuration, and extension patterns
- [ ] T052 Update main `README.md` with link to DI integration guide and 3-line quick reference

---

## Phase 7: Documentation & Polish

**Purpose**: Complete documentation and prepare for release

- [ ] T053 Create `MIGRATION.md` for developers integrating from prior setup patterns to `AddSigilValidation()`
- [ ] T054 Update `docs/architecture.md` with DI integration strategy and service lifetime decisions
- [ ] T055 Add inline code examples to XML docstrings for all public extension methods
- [ ] T056 Create troubleshooting guide for common DI registration issues and solutions
- [ ] T057 Verify no `proofBytes` appear in any log statements or error messages via code review
- [ ] T058 Update `CHANGELOG.md` with DI integration feature, all new overloads, and deprecations (if any)
- [ ] T059 Validate compliance with SDK constitution (schema-first, deterministic failures, immutable registries, no breaking changes)
- [ ] T060 [P] Run full test suite for all existing functionality (regression testing)
- [ ] T061 [P] Run full test suite for new DI integration features (T019-T050)

---

## Phase 8: Final Verification & Testing

**Purpose**: Comprehensive testing before merge

- [ ] T062 [P] Build and run sample project successfully on .NET 10
- [ ] T063 [P] Build and run sample project successfully on .NET 8
- [ ] T064 [P] Verify NuGet package dependency versions are .NET 10 compatible
- [ ] T065 [P] Generate API documentation and verify no broken links
- [ ] T066 Run static analysis tool (if configured) and resolve any new issues
- [ ] T067 Code review: verify all edge cases handled per spec clarifications
- [ ] T068 Code review: verify immutability of registries enforced
- [ ] T069 Code review: verify all required exceptions throw with clear, actionable messages
- [ ] T070 Performance profiling: schema compilation, service resolution, validation latency
- [ ] T071 Update coverage reports if applicable
- [ ] T073 Performance profiling: schema compilation, service resolution, validation latency
- [ ] T074 Update coverage reports if applicable

**Checkpoint**: Ready for merge and release

---

## Dependency Summary

### User Story Dependencies
- **US1 (Basic Setup)** blocks: Nothing - MVP
- **US2 (Diagnostics)** depends on: US1 (leverages base setup)
- **US3 (Extensions)** depends on: US1 (leverages base setup)

### Implementation Task Dependencies
- **Phase 1 (Setup)**: No dependencies
- **Phase 2 (Foundational)**: Depends on Phase 1
- **Phases 3-5 (User Stories)**: Depend on Phases 1-2 ✓
- **Phase 6 (Edge Cases)**: Depend on all user story implementations
- **Phase 7 (Documentation)**: Depend on all implementations
- **Phase 8 (Verification)**: Final validation pass

### Parallel Execution Opportunities
- **Phase 1**: All tasks can run in parallel (independent extension methods)
- **Phase 2**: T007-T013 can run in parallel (independent registrations) - T014 depends on all
- **Phase 3 (US1)**: T015-T022 can run in parallel (sample and tests independent)
- **Phase 4 (US2)**: T024-T032 can run in parallel (logging config independent)
- **Phase 5 (US3)**: T034-T045 can run in parallel (custom systems independent)
- **Phase 6 (Edge Cases)**: T047-T053 can run in parallel (independent tests)
- **Phase 8 (Verification)**: T063-T067, T069 can run in parallel (no dependencies)

---

## Implementation Strategy

### MVP Scope (Minimum Viable Product)
- **Phase 1**: Setup (T001-T006)
- **Phase 2**: Foundational (T007-T014)
- **Phase 3**: User Story 1 (T015-T023)
- **Phase 6, T047-T048**: Critical edge cases

**Deliverable**: Developers can integrate Sigil validation with 3-5 lines of code; basic validation works with default config

### Phase 2 Expansion (Production Ready)
- Add Phase 4 (User Story 2 - Diagnostics): Production-grade logging configuration
- Add Phase 6 (remaining tests): Comprehensive edge case coverage
- Add Phase 7 (Documentation): Integration guide and troubleshooting

### Phase 3 Full Release
- Add Phase 5 (User Story 3 - Extensions): Custom proof systems and handlers
- Add Phase 8 (Verification): Final comprehensive testing

---

## Success Criteria Mapping

| Success Criterion | Supporting Tasks | Verified By |
|---|---|---|
| SC-001: 5 min setup, ≤5 lines | T014-T018, T051 | T022, code review |
| SC-002: Schema compile <100ms | T009, T049 | T049 (performance test) |
| SC-003: 90% success on first attempt | T017-T018, T051 | Developer feedback |
| SC-004: Memory <5MB | T001-T013, T050 | T050 (memory test) |
| SC-005: 2 lines per extension | T031-T042, T043 | Code review, documentation |
| SC-006: Works in all host types | T048 | T048 (integration test) |
| SC-007: 4.5+/5 doc rating | T051-T058 | Developer survey |
| SC-008: Zero DI resolution failures | T019-T022 | T067 (code review) |
| SC-009: Sample <50 lines (excl boilerplate) | T014-T016 | Code review |
| SC-010: Support tickets -75% | Delivery & monitoring | Post-release metrics |

---

## Notes for Implementation

- **Spec Reference**: All tasks reference Spec 001 (Proof Envelope Format) and Spec 002 (Validation API) as already-complete dependencies
- **Constitution Compliance**: Tasks T004, T034-T035, T044, T046-T047 explicitly implement constitution requirements
- **Testing First**: Consider running contract/unit tests before implementation to verify spec correctness
- **Documentation as Code**: Keep examples in XML docstrings updated alongside implementation
- **Performance**: T049-T050 must run before release to validate SC-002 and SC-004 targets
