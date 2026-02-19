# Implementation Plan: Statement Handler Contract & license:v1 Statement Definition

**Branch**: `004-statement-handler-contract` | **Date**: 2026-02-19 | **Spec**: [specs/004-statement-handler-contract/spec.md](spec.md)
**Input**: Feature specification from [specs/004-statement-handler-contract/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Formalize statement extensibility by tightening the `IStatementHandler` contract, defining a normative `license:v1` statement definition, and codifying expiry as statement semantics while keeping expiry execution in the validator pipeline. Implementation will preserve deterministic, result-object validation and immutable DI registries, with compatibility for building using .NET 10 SDK.

## Technical Context

**Language/Version**: C#; SDK built with .NET 10 SDK, library target remains `net8.0` (compatible runtime contract)  
**Primary Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, Corvus.Json.Validator  
**Storage**: N/A  
**Testing**: xUnit + Microsoft.NET.Test.Sdk (`dotnet test`)  
**Target Platform**: Cross-platform .NET runtime; validated on .NET 10 SDK toolchain  
**Project Type**: Single SDK library  
**Performance Goals**: Statement validation remains sub-10ms for typical `publicInputs`; no regression to existing p95 validation targets  
**Constraints**: Deterministic/fail-closed behavior, offline-only validation, no `proofBytes` logging, no breaking API changes without versioning notes  
**Scale/Scope**: One SDK package (`src/Sigil.Sdk`) plus tests (`tests/Sigil.Sdk.Tests`)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (existing pipeline order retained)
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (registry behavior unchanged)
- Validation failures MUST return a result object (no throws). **PASS** (contract remains result-object oriented)
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (no new logging of proof material)
- Registries MUST be DI-configured and immutable after construction. **PASS** (no runtime mutability introduced)
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (spec-driven; no breaking API required)

## Project Structure

### Documentation (this feature)

```text
specs/004-statement-handler-contract/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── statement-handler-contract.md
│   ├── license-v1-statement.md
│   └── expiry-evaluation.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
└── Sigil.Sdk/
    ├── DependencyInjection/
    ├── Registries/
    ├── Statements/
    └── Validation/

tests/
└── Sigil.Sdk.Tests/
    ├── DependencyInjection/
    ├── Statements/
    └── Validation/
```

**Structure Decision**: Keep the existing single-library layout and add statement-contract behavior in `Statements` and `Validation` with focused tests in `tests/Sigil.Sdk.Tests`.

## Complexity Tracking

No constitution violations.

## Phase 0 — Research Output

- [specs/004-statement-handler-contract/research.md](research.md)

## Phase 1 — Design Output

- [specs/004-statement-handler-contract/data-model.md](data-model.md)
- [specs/004-statement-handler-contract/contracts/statement-handler-contract.md](contracts/statement-handler-contract.md)
- [specs/004-statement-handler-contract/contracts/license-v1-statement.md](contracts/license-v1-statement.md)
- [specs/004-statement-handler-contract/contracts/expiry-evaluation.md](contracts/expiry-evaluation.md)
- [specs/004-statement-handler-contract/quickstart.md](quickstart.md)

## Constitution Check (Post-Design)

- Schema validation before proof verification: **PASS**
- Deterministic failure results: **PASS**
- `proofBytes` never logged/emitted: **PASS**
- Immutable DI registries: **PASS**
- No breaking API changes without versioning notes: **PASS**
