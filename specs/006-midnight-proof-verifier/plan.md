# Implementation Plan: Midnight Proof System Verifier

**Branch**: `006-midnight-proof-verifier` | **Date**: 2026-02-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from [specs/006-midnight-proof-verifier/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement the first concrete proof system verifier for the Sigil SDK: Midnight (`midnight-zk-v1`). This feature extends the proof system verifier contract (Spec 005) with deterministic, offline cryptographic verification for Midnight zero-knowledge proofs, supporting `urn:sigil:statement:license:v1` initially. Design emphasizes fail-closed behavior, strict proof confidentiality (no `proofBytes` logging), deterministic failure mapping (`Invalid` for crypto failures, `Error` for internal faults), and safe verifier-state reuse within the Spec 002 validation pipeline performance budget.

## Technical Context

**Language/Version**: C#; SDK built with .NET 10 SDK, library target `net8.0` (runtime compatibility baseline)  
**Primary Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, existing Spec 005 proof-system contracts (`IProofSystemVerifier`, `ProofVerificationOutcome`), Spec 002 validation pipeline integration  
**Storage**: N/A (verification-only scope)  
**Testing**: xUnit + Microsoft.NET.Test.Sdk via `dotnet test`; conformance vectors (known-valid, known-invalid, internal-error scenarios)  
**Target Platform**: Cross-platform .NET runtime; offline verification environments
**Project Type**: Single SDK library (`src/Sigil.Sdk`)  
**Performance Goals**: End-to-end validation p95 < 1s for envelopes ≤ 10KB (Spec 002 SC-004); Midnight verification ≤ 60% of that p95 budget  
**Constraints**: Deterministic verification outcomes, offline-only operation, fail-closed behavior, no `proofBytes` logging/emission, immutable DI registries, thread-safe verifier-state reuse  
**Scale/Scope**: Single SDK package extension; first concrete proof system implementation; `license:v1` statement support initially

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (Spec 002 pipeline ordering preserved; Midnight verifier runs after schema gate)
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (Spec 005 registry enforcement; Midnight verifier only handles registered `midnight-zk-v1`)
- Validation failures MUST return a result object (no throws). **PASS** (Spec 006 FR-005, FR-007: deterministic failure mapping to `Invalid`/`Error` statuses via result objects)
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (Spec 006 FR-011, FR-012, FR-013: strict redaction, opt-in diagnostics)
- Registries MUST be DI-configured and immutable after construction. **PASS** (Spec 006 FR-002: Midnight verifier registered via DI; Spec 005 immutable registry contracts)
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (This feature is spec-driven and additive; first concrete proof system, no breaking changes)

## Project Structure

### Documentation (this feature)

```text
specs/006-midnight-proof-verifier/
├── spec.md
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── midnight-verifier-behavior.md  # Phase 1 output
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
src/Sigil.Sdk/
├── Proof/
│   ├── IProofSystemVerifier.cs          # Spec 005 contract (existing)
│   ├── ProofSystemIds.cs                # Canonical identifiers (existing; extended)
│   ├── ProofVerificationContext.cs      # Spec 005 contract (existing)
│   ├── ProofVerificationOutcome.cs      # Spec 005 contract (existing)
│   └── MidnightZkV1ProofSystemVerifier.cs  # Spec 006 implementation
├── Registries/
│   └── ImmutableProofSystemRegistry.cs  # Spec 005 (existing)
├── DependencyInjection/
│   └── ServiceCollectionExtensions.cs   # Spec 003 (existing; extended for Midnight)
├── Validation/
│   ├── LicenseValidator.cs              # Spec 002 (existing; may need Error mapping refinement)
│   ├── LicenseFailureCode.cs            # Spec 002 (existing)
│   ├── ValidationOptions.cs             # Spec 003 (existing; extended with AddMidnightZkV1ProofSystem)
│   └── FailureClassification.cs         # Spec 002 (existing)
└── ...

tests/Sigil.Sdk.Tests/
├── Validation/
│   ├── MidnightProofConformanceTests.cs       # New
│   ├── MidnightVerifierInitializationTests.cs # New
│   ├── Conformance/
│   │   ├── README.md                          # New
│   │   ├── MidnightConformanceVectors.cs      # New
│   │   └── Vectors/
│   │       ├── license-v1-valid.json          # New
│   │       ├── license-v1-invalid.json        # New
│   │       └── license-v1-internal-error.json # New
│   └── LicenseValidatorCryptoTests.cs         # Existing
├── DependencyInjection/
│   └── ServiceCollectionExtensionsTests.cs    # Existing (may extend)
├── Logging/
│   └── ValidationLoggingTests.cs              # Existing (extend with Midnight redaction tests)
└── Performance/
    └── ValidationPerformanceBenchmarks.cs     # Existing (extend with Midnight budget assertions)
```

**Structure Decision**: Midnight verifier implementation integrates into the existing single-library SDK layout established by Specs 001-005. Core verifier logic resides in `src/Sigil.Sdk/Proof/MidnightZkV1ProofSystemVerifier.cs`. Conformance test infrastructure is added under `tests/Sigil.Sdk.Tests/Validation/Conformance/` to support vector-driven validation required by FR-014 and FR-015. Existing validator, DI, and logging tests are extended rather than duplicated.

## Complexity Tracking

No constitution violations.

## Phase 0 — Research Output

- [research.md](research.md)

## Phase 1 — Design Output

- [data-model.md](data-model.md)
- [contracts/midnight-verifier-behavior.md](contracts/midnight-verifier-behavior.md)
- [quickstart.md](quickstart.md)

## Phase 0 Research Resolution: Midnight Library + WASM Bridge Strategy

**Status**: ✅ **RESOLVED** — T012-T024 (Phase 2.5 PoC) unblocked

**Decision**: Use **Wasmtime.NET** to invoke `@midnight-ntwrk/proof-verification` WASM library

**Key Findings**:
- **Library**: `@midnight-ntwrk/proof-verification` (NPM package by IOG)
- **Bridge**: Wasmtime.NET (`dotnet add package Wasmtime` v16+)
- **Performance**: 20-120ms per proof verification ✅ (SC-004 budget: <600ms with 80% headroom)
- **Compatibility**: .NET 8.0 fully supported; cross-platform (Windows/Linux/macOS/ARM)
- **Maintainability**: Bytecode Alliance maintains Wasmtime; production-tested by Cloudflare, Fastly, Netlify

**Implementation Tasks Unblocked**:
- T012-T024: Phase 2.5 PoC validates bridge before production (T027+)
- T027: Implement offline Midnight proof verification via Wasmtime.NET bridge
- T028-T030: Proceed with deterministic failure mapping, statement validation

**See**: [research.md](research.md#q6-which-midnight-libraries-provide-zk-verification-and-how-should-wasm-components-be-bridged-to-c) Q6 for detailed analysis, or [../RESEARCH_FINDINGS_MIDNIGHT_VERIFICATION.md](../../RESEARCH_FINDINGS_MIDNIGHT_VERIFICATION.md) (repo root) for full technical deep-dive.

## Phase 2.5 (PoC) Exit Criteria

**PoC Complete When All Gates Pass**:

1. **Latency Validation**: All 10+ measured proof verifications complete in <120ms p95 ✅
2. **WASM Integration**: Extracted WASM binary embeds correctly in SDK build and loads without errors ✅
3. **Function Signatures**: Midnight WASM function signatures reverse-engineered and documented in docs/ ✅
4. **Test Vectors**: Minimum 3 conformance vectors (valid/invalid/error) acquired per FR-015 ✅

**Exit Blocker**: If any gate fails 8+ times during T020-T022, reconvene team to evaluate bridge strategy vs alternatives.

**Note on Spec 001 Consolidation**: Specs folder contains both `/specs/001-midnight-proof-verifier/` (legacy) and `/specs/006-midnight-proof-verifier/` (authoritative). Post-implementation: remove or redirect Spec 001 to Spec 006; maintain single source of truth for Midnight verifier requirements.

## Implementation Checklist Anchors

- Setup complete: `T001`-`T011`
- Foundational complete: (T001-T011 overlap)
- PoC complete: `T012`-`T024`
- MVP (US1) complete: `T025`-`T030`
- US2 complete: `T031`-`T036`
- US3 complete: `T037`-`T041`
- Polish complete: `T042`-`T045`

## Constitution Check (Post-Design)

- Schema validation before proof verification: **PASS**
- Unknown identifiers fail deterministically: **PASS**
- Result-object validation failures: **PASS**
- `proofBytes` never logged/emitted: **PASS**
- Immutable DI-configured registries: **PASS**
- No undocumented breaking changes: **PASS**

