# Implementation Plan: Proof System Verifier Contract

**Branch**: `005-proof-system-verifier-contract` | **Date**: 2026-02-20 | **Spec**: [specs/005-proof-system-verifier-contract/spec.md](spec.md)
**Input**: Feature specification from [specs/005-proof-system-verifier-contract/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Define a deterministic, fail-closed proof-system verifier contract with Midnight-first support (`midnight-zk-v1`) and DI-based extensibility for future proof systems. Design work focuses on stable verifier abstraction boundaries, immutable registry resolution, deterministic failure/status mapping compatible with Spec 002 pipeline semantics, and strict proof confidentiality in diagnostics.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C#; SDK built with .NET 10 SDK, library target remains `net8.0` (runtime compatibility baseline)  
**Primary Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, Corvus.Json.Validator  
**Storage**: N/A  
**Testing**: xUnit + Microsoft.NET.Test.Sdk (`dotnet test`)  
**Target Platform**: Cross-platform .NET runtime; offline verification environments  
**Project Type**: Single SDK library  
**Performance Goals**: Maintain end-to-end validation p95 < 1s for envelopes <= 10 KB (aligns Spec 002 / Spec 005 SC-006)  
**Constraints**: Deterministic outcomes, fail-closed behavior, schema-before-proof pipeline ordering, immutable DI registries, no `proofBytes` logging/emission, offline-only verifier operation  
**Scale/Scope**: One SDK package (`src/Sigil.Sdk`) with focused tests in `tests/Sigil.Sdk.Tests`; contract supports one built-in proof system now plus extension points

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (Spec 005 FR-013 preserves stage precedence and non-override)
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (Spec 005 FR-008, FR-007a; Spec 002 handles statement IDs)
- Validation failures MUST return a result object (no throws). **PASS** (status/failure mapping retained; only cancellation propagates by design)
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (Spec 005 FR-015, FR-016)
- Registries MUST be DI-configured and immutable after construction. **PASS** (Spec 005 FR-006, FR-007, FR-009)
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (feature is spec-driven and additive)

## Project Structure

### Documentation (this feature)

```text
specs/005-proof-system-verifier-contract/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── proof-system-verifier.openapi.yaml
└── tasks.md
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
src/
└── Sigil.Sdk/
  ├── DependencyInjection/
  ├── Registries/
  ├── Proof/
  ├── Statements/
  └── Validation/

tests/
└── Sigil.Sdk.Tests/
  ├── DependencyInjection/
  ├── Validation/
  └── Performance/
```

**Structure Decision**: Keep the existing single-library SDK layout and implement proof-system contract additions in `Proof`, `Registries`, `DependencyInjection`, and `Validation`, with deterministic mapping and conformance tests in `tests/Sigil.Sdk.Tests`.

## Complexity Tracking

No constitution violations.

## Phase 0 — Research Output

- [specs/005-proof-system-verifier-contract/research.md](research.md)

## Phase 1 — Design Output

- [specs/005-proof-system-verifier-contract/data-model.md](data-model.md)
- [specs/005-proof-system-verifier-contract/contracts/proof-system-verifier.openapi.yaml](contracts/proof-system-verifier.openapi.yaml)
- [specs/005-proof-system-verifier-contract/quickstart.md](quickstart.md)

## Implementation Checklist Anchors

- Setup complete: `T001`-`T003`
- Foundational complete: `T004`-`T009`
- MVP (US1) complete: `T010`-`T015`
- US2 complete: `T016`-`T020`
- US3 complete: `T021`-`T025`
- Polish complete: `T026`-`T031`

## Constitution Check (Post-Design)

- Schema validation before proof verification: **PASS**
- Unknown identifiers fail deterministically: **PASS**
- Result-object validation failures: **PASS**
- `proofBytes` never logged/emitted: **PASS**
- Immutable DI-configured registries: **PASS**
- No undocumented breaking changes: **PASS**
