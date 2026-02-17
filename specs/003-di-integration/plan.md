# Implementation Plan: DI Integration & Defaults

**Branch**: `003-di-integration` | **Date**: 2026-02-17 | **Spec**: [specs/003-di-integration/spec.md](specs/003-di-integration/spec.md)
**Input**: Feature specification from `/specs/003-di-integration/spec.md`

## Summary

Deliver a drop-in DI registration method (`AddSigilValidation`) that wires the validator, immutable registries, schema validator, and default clock with secure logging defaults, plus clear extension points for custom proof systems and statements. Provide a minimal quickstart and a sample for 3-5 line setup.

## Technical Context

**Language/Version**: C# with .NET 10 SDK (library currently targets `net8.0`)  
**Primary Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, Corvus.Json.Validator  
**Storage**: N/A  
**Testing**: xUnit (Microsoft.NET.Test.Sdk)  
**Target Platform**: .NET applications (ASP.NET Core, worker, console)  
**Project Type**: Single library  
**Performance Goals**: Schema compilation under 100 ms; minimal startup overhead; memory under 5 MB  
**Constraints**: Deterministic validation, offline-capable, no `proofBytes` logging, immutable registries, .NET 10 compatible  
**Scale/Scope**: SDK integration surface with DI registrations, options, and sample usage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (Spec 002 enforced; no changes)
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (registries immutable; duplicates fail fast)
- Validation failures MUST return a result object (no throws). **PASS** (unchanged API behavior)
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (logging policy unchanged)
- Registries MUST be DI-configured and immutable after construction. **PASS** (explicit DI registration)
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (no breaking change planned)

## Project Structure

### Documentation (this feature)

```text
specs/003-di-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── Sigil.Sdk/
    ├── DependencyInjection/
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

samples/
└── [new minimal app for DI integration]
```

**Structure Decision**: Single library with tests and a new minimal sample under `samples/`.

## Phase 0: Research

- Confirm DI extension method behavior (duplicate calls, duplicate registrations) aligned with deterministic behavior.
- Validate options/logging integration approach for minimal setup and host-configured logging.
- Confirm service lifetime expectations for schema validator and registries.

**Output**: [specs/003-di-integration/research.md](specs/003-di-integration/research.md)

## Phase 1: Design & Contracts

### Data Model

- Define configuration entities: `ValidationOptions`, proof system registrations, statement registrations, and immutable registries.
- Document schema validator and clock roles.

**Output**: [specs/003-di-integration/data-model.md](specs/003-di-integration/data-model.md)

### Contracts

- Define public API contract for `AddSigilValidation` and configuration extension points.
- Document error behaviors for duplicate registration and missing dependencies.

**Output**: [specs/003-di-integration/contracts/public-api.md](specs/003-di-integration/contracts/public-api.md)

### Quickstart

- Provide minimal 3-5 line DI setup snippet and optional diagnostics example.

**Output**: [specs/003-di-integration/quickstart.md](specs/003-di-integration/quickstart.md)

### Agent Context Update

- Run `.specify/scripts/bash/update-agent-context.sh copilot` after Phase 1 artifacts are created.

## Constitution Check (Post-Design)

- Schema validation before proof verification. **PASS**
- Unknown `proofSystem` or `statementId` deterministic failure. **PASS**
- Result-object failures, no exceptions for validation failures. **PASS**
- `proofBytes` never logged. **PASS**
- Registries DI-configured and immutable. **PASS**
- No breaking changes without ADR/spec notes. **PASS**

## Phase 2: Planning

- Break down implementation tasks for DI registration, options handling, and sample app.
- Identify test coverage additions (DI registration, duplicate registration errors, sample build).

**Output**: `tasks.md` (created by `/speckit.tasks`)
