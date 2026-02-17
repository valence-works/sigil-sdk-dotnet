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

- [ ] T001 Create `ServiceCollectionExtensions.AddSigilValidation()` extension method overloads in `src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T002 Implement duplicate-call guard in `AddSigilValidation()` to throw `InvalidOperationException` on second call
- [ ] T003 Create `ValidationSigilOptions` configuration builder class in `src/Sigil.Sdk/DependencyInjection/ValidationSigilOptions.cs` with `AddProofSystem()` and `AddStatementHandler()` methods
- [ ] T004 Implement duplicate identifier detection in `ValidationSigilOptions` to throw `InvalidOperationException` immediately
- [ ] T005 Create immutable registry construction logic in `src/Sigil.Sdk/Registries/` to build registries from collected registrations
- [ ] T006 Implement registry immutability enforcement (throw on modification attempts after container build) in registry internals

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core DI registration wiring and options handling

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 Register `ISigilValidator` as singleton in `AddSigilValidation()` with all dependencies resolved
- [ ] T008 Register immutable `IProofSystemRegistry` as singleton with empty default state
- [ ] T009 Register immutable `IStatementRegistry` as singleton with empty default state
- [ ] T010 Register `IProofEnvelopeSchemaValidator` as singleton (compiled once at startup)
- [ ] T011 Register `IClock` as singleton with `SystemClock` as default implementation
- [ ] T012 Register `ValidationOptions` as singleton with production-ready defaults (diagnostics off, no sensitive logging)
- [ ] T013 Add validation in `AddSigilValidation()` to throw `InvalidOperationException` if required dependencies are missing or misconfigured
- [ ] T014 Update `ServiceCollectionExtensions.cs` to handle optional configuration delegate parameter with null safety

**Checkpoint**: Foundation ready - all core services registered and validated

---

## Phase 3: User Story 1 - Basic Validator Setup (Priority: P1) 🎯 MVP

**Goal**: Developers can call `services.AddSigilValidation()` and immediately use `ISigilValidator` with default configuration

**Independent Test**: Create minimal .NET app, register services with `AddSigilValidation()`, inject `ISigilValidator`, validate a proof envelope, receive result without errors

### Implementation for User Story 1

- [ ] T015 [P] [US1] Create minimal sample app in `samples/MinimalDiSample/Program.cs` demonstrating 3-5 line setup
- [ ] T016 [P] [US1] Create sample `Startup.cs` or `Program.cs` with `services.AddSigilValidation()` registration
- [ ] T017 [P] [US1] Add controller or service in sample that injects and uses `ISigilValidator` for validation
- [ ] T018 [US1] Update `src/Sigil.Sdk/DependencyInjection/ServiceCollectionExtensions.cs` with clear XML documentation for `AddSigilValidation()` with minimal setup example
- [ ] T019 [US1] Update README or create `QUICKSTART.md` with 5-line integration example and link to sample
- [ ] T020 [P] [US1] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying `AddSigilValidation()` registers all required services
- [ ] T021 [P] [US1] Add unit test verifying `ISigilValidator` can be resolved from DI container after `AddSigilValidation()`
- [ ] T022 [P] [US1] Add unit test verifying validator executes successfully with default configuration
- [ ] T023 [US1] Add integration test in sample project verifying end-to-end validation with valid and invalid envelopes

**Checkpoint**: User Story 1 complete - basic setup works with default configuration

---

## Phase 4: User Story 2 - Custom Diagnostics Configuration (Priority: P2)

**Goal**: Developers can configure logging and diagnostics options during registration without affecting core validation behavior

**Independent Test**: Configure `options.EnableDiagnostics = false`, perform validation, verify no diagnostic output; configure `options.EnableDiagnostics = true`, perform validation, verify appropriate diagnostic logging

### Implementation for User Story 2

- [ ] T024 [P] [US2] Extend `ValidationOptions` in `src/Sigil.Sdk/Validation/ValidationOptions.cs` with additional configuration properties for logging behavior (e.g., `LogFailureDetails`, `LogSessionInfo`)
- [ ] T025 [P] [US2] Update `ValidationSigilOptions` builder to accept and apply logging configuration options
- [ ] T026 [P] [US2] Update `LicenseValidator` to respect `ValidationOptions` logging configuration when writing to logs
- [ ] T027 [US2] Update `ValidationLogging.cs` helpers to check `ValidationOptions` before logging sensitive data
- [ ] T028 [P] [US2] Add implementation in `AddSigilValidation()` to pass options to validator correctly
- [ ] T029 [P] [US2] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying configuration delegate is applied
- [ ] T030 [P] [US2] Add unit test verifying `options.LogFailureDetails = false` prevents detailed logging
- [ ] T031 [P] [US2] Add integration test verifying custom diagnostics configuration is respected during validation
- [ ] T032 [US2] Add sample code in `samples/MinimalDiSample/` demonstrating diagnostics configuration option
- [ ] T033 [US2] Update documentation with diagnostics configuration examples and best practices

**Checkpoint**: User Stories 1 & 2 complete - diagnostics configuration working independently

---

## Phase 5: User Story 3 - Extension with Custom Systems (Priority: P3)

**Goal**: Developers can register custom proof system verifiers and statement handlers and have them integrated seamlessly into validation

**Independent Test**: Register custom proof system and statement handler via `options.AddProofSystem()` and `options.AddStatementHandler()`, validate envelope containing custom types, verify custom implementation is invoked

### Implementation for User Story 3

- [ ] T034 [P] [US3] Implement `ValidationSigilOptions.AddProofSystem(identifier, verifier)` method that adds verifier to internal collection and validates no duplicates
- [ ] T035 [P] [US3] Implement `ValidationSigilOptions.AddStatementHandler(handler)` method that adds handler to internal collection
- [ ] T036 [P] [US3] Update registry construction in Phase 2 to include custom systems from `ValidationSigilOptions`
- [ ] T037 [P] [US3] Add duplicate identifier detection in `AddProofSystem()` to throw `InvalidOperationException` immediately
- [ ] T038 [P] [US3] Add duplicate identifier detection in `AddStatementHandler()` to throw `InvalidOperationException` immediately
- [ ] T039 [P] [US3] Update XML documentation in `ValidationSigilOptions` with clear examples for `AddProofSystem()` and `AddStatementHandler()`
- [ ] T040 [P] [US3] Add unit test in `tests/Sigil.Sdk.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` verifying custom proof system can be registered
- [ ] T041 [P] [US3] Add unit test verifying duplicate proof system identifier throws `InvalidOperationException`
- [ ] T042 [P] [US3] Add unit test verifying custom statement handler can be registered
- [ ] T043 [P] [US3] Add unit test verifying duplicate statement identifier throws `InvalidOperationException`
- [ ] T044 [US3] Add integration test verifying custom verifier/handler is invoked during validation when applicable
- [ ] T045 [P] [US3] Add sample code in `samples/MinimalDiSample/` demonstrating custom system registration and extension usage
- [ ] T046 [US3] Update documentation with extension point examples and patterns for developers adding custom systems

**Checkpoint**: All user stories complete - extension points working and documented

---

## Phase 6: Edge Cases & Error Handling

**Purpose**: Implement and test all edge cases identified in spec

- [ ] T047 [P] Add unit test verifying second call to `AddSigilValidation()` throws `InvalidOperationException` with clear message
- [ ] T048 [P] Add unit test verifying missing required dependencies throw `InvalidOperationException` during registration with guidance
- [ ] T049 [P] Add unit test verifying exception from custom verifier during registration is wrapped as `InvalidOperationException`
- [ ] T050 [P] Add unit test verifying runtime modification of registries after container build throws `InvalidOperationException`
- [ ] T051 [P] Add integration test for multiple application hosting scenarios (ASP.NET Core, console app, worker service)
- [ ] T052 [P] Add performance test verifying schema compilation completes in under 100 milliseconds
- [ ] T053 [P] Add memory profiling test verifying service overhead under 5 MB with default configuration

---

## Phase 7: Documentation & Polish

**Purpose**: Complete documentation and prepare for release

- [ ] T054 Create comprehensive integration guide in `docs/DI_INTEGRATION.md` with minimal setup, options configuration, and extension patterns
- [ ] T055 Update main `README.md` with link to DI integration guide and 3-line quick reference
- [ ] T056 Create `MIGRATION.md` for developers integrating from prior setup patterns to `AddSigilValidation()`
- [ ] T057 Update `docs/architecture.md` with DI integration strategy and service lifetime decisions
- [ ] T058 Add inline code examples to XML docstrings for all public extension methods
- [ ] T059 Create troubleshooting guide for common DI registration issues and solutions
- [ ] T060 Verify no `proofBytes` appear in any log statements or error messages via code review
- [ ] T061 Update `CHANGELOG.md` with DI integration feature, all new overloads, and deprecations (if any)
- [ ] T062 Validate compliance with SDK constitution (schema-first, deterministic failures, immutable registries, no breaking changes)

---

## Phase 8: Final Verification & Testing

**Purpose**: Comprehensive testing before merge

- [ ] T063 [P] Run full test suite for all existing functionality (regression testing)
- [ ] T064 [P] Run full test suite for new DI integration features (T020-T053)
- [ ] T065 [P] Build and run sample project successfully on .NET 10
- [ ] T066 [P] Build and run sample project successfully on .NET 8
- [ ] T067 [P] Verify NuGet package dependency versions are .NET 10 compatible
- [ ] T068 [P] Generate API documentation and verify no broken links
- [ ] T069 Run static analysis tool (if configured) and resolve any new issues
- [ ] T070 Code review: verify all edge cases handled per spec clarifications
- [ ] T071 Code review: verify immutability of registries enforced
- [ ] T072 Code review: verify all required exceptions throw with clear, actionable messages
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
| SC-001: 5 min setup, ≤5 lines | T015-T019, T054 | T023, code review |
| SC-002: Schema compile <100ms | T010, T052 | T052 (performance test) |
| SC-003: 90% success on first attempt | T018-T019, T054 | Developer feedback |
| SC-004: Memory <5MB | T001-T014, T053 | T053 (memory test) |
| SC-005: 2 lines per extension | T034-T045, T046 | Code review, documentation |
| SC-006: Works in all host types | T051 | T051 (integration test) |
| SC-007: 4.5+/5 doc rating | T054-T059 | Developer survey |
| SC-008: Zero DI resolution failures | T020-T023 | T070 (code review) |
| SC-009: Sample <50 lines (excl boilerplate) | T015-T017 | Code review |
| SC-010: Support tickets -75% | Delivery & monitoring | Post-release metrics |

---

## Notes for Implementation

- **Spec Reference**: All tasks reference Spec 001 (Proof Envelope Format) and Spec 002 (Validation API) as already-complete dependencies
- **Constitution Compliance**: Tasks T004, T037-T038, T047, T049-T050 explicitly implement constitution requirements
- **Testing First**: Consider running contract/unit tests before implementation to verify spec correctness
- **Documentation as Code**: Keep examples in XML docstrings updated alongside implementation
- **Performance**: T052-T053 must run before release to validate SC-002 and SC-004 targets
