# Implementation Plan: SDK Validation API

**Branch**: `002-sdk-validation-api` | **Date**: 2026-02-17 | **Spec**: [specs/002-sdk-validation-api/spec.md](spec.md)
**Input**: Feature specification from [specs/002-sdk-validation-api/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a deterministic, result-object validation API for Spec 001 proof envelopes. The plan enforces schema-first validation, immutable DI registries, and fail-closed semantics with stable failure codes, using a single compiled JSON schema and a strict validation pipeline order.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# with .NET 8 (`net8.0`)  
**Primary Dependencies**: Corvus.Json.Validator, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions  
**Storage**: N/A  
**Testing**: xUnit with Microsoft.NET.Test.Sdk  
**Target Platform**: .NET 8 (cross-platform)  
**Project Type**: single SDK library  
**Performance Goals**: p95 < 1s for envelopes <= 10 KB (SC-004)  
**Constraints**: offline-capable, deterministic results, schema-first validation  
**Scale/Scope**: single SDK package with unit/performance tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

- Schema validation MUST run before proof verification. **PASS**
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS**
- Validation failures MUST return a result object (no throws). **PASS**
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS**
- Registries MUST be DI-configured and immutable after construction. **PASS**
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS**

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
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
  ├── Contracts/
  ├── DependencyInjection/
  ├── Envelope/
  ├── Logging/
  ├── Proof/
  ├── Registries/
  ├── Schema/
  ├── Statements/
  ├── Time/
  └── Validation/

tests/
└── Sigil.Sdk.Tests/
  ├── Logging/
  ├── Performance/
  └── Validation/
```

**Structure Decision**: Single SDK library with tests under `tests/`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitution violations.

## Phase 0 — Research Output

- [specs/002-sdk-validation-api/research.md](research.md)

## Phase 1 — Design Output

- [specs/002-sdk-validation-api/data-model.md](data-model.md)
- [specs/002-sdk-validation-api/contracts/public-api.md](contracts/public-api.md)
- [specs/002-sdk-validation-api/contracts/failure-codes.md](contracts/failure-codes.md)
- [specs/002-sdk-validation-api/contracts/logging-policy.md](contracts/logging-policy.md)
- [specs/002-sdk-validation-api/quickstart.md](quickstart.md)

## Constitution Check (Post-Design)

- Schema validation before proof verification: **PASS**
- Deterministic failure results: **PASS**
- `proofBytes` never logged/emitted: **PASS**
- Immutable DI registries: **PASS**
