# Implementation Plan: SDK Validation API

**Branch**: `002-sdk-validation-api` | **Date**: 2026-02-16 | **Spec**: [specs/002-sdk-validation-api/spec.md](specs/002-sdk-validation-api/spec.md)
**Input**: Feature specification from /specs/002-sdk-validation-api/spec.md

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Deliver an offline, deterministic Proof Envelope validation API for Sigil SDK that validates Spec 001 envelopes with a result-object model (no exceptions for expected failures). Validation must be schema-first, fail closed, never log `proofBytes`, use immutable DI-provided registries for `proofSystem` and `statementId`, evaluate expiry (`publicInputs.expiresAt`) and return `Expired` when appropriate, and expose `ValidateAsync(string)` and `ValidateAsync(Stream)` entry points.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 8 (planned SDK target)  
**Primary Dependencies**: System.Text.Json; Microsoft.Extensions.DependencyInjection; Microsoft.Extensions.Logging; Corvus.Json.Validator (Draft 2020-12 JSON Schema)  
**Storage**: N/A  
**Testing**: xUnit (planned)  
**Target Platform**: Cross-platform .NET 8 (Windows/macOS/Linux)  
**Project Type**: Single SDK library  
**Performance Goals**: Validate <=10 KB envelopes within 1 second offline (per spec success criteria)  
**Constraints**: Offline-only, deterministic failures, schema-before-crypto, fail-closed, no `proofBytes` logging, diagnostics opt-in  
**Scale/Scope**: Spec 001 envelope validation plus extensible registries for future statements and proof systems

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (Spec FR-005).
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (Spec FR-006, FR-007, FR-013).
- Validation failures MUST return a result object (no throws). **PASS** (Spec FR-002).
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (Spec FR-011).
- Registries MUST be DI-configured and immutable after construction. **PASS** (Spec FR-007).
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (no breaking change proposed).

## Project Structure

### Documentation (this feature)

```text
specs/002-sdk-validation-api/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── public-api.md
│   ├── failure-codes.md
│   └── logging-policy.md
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

tests/
```

**Structure Decision**: Documentation and contracts are produced in `specs/002-sdk-validation-api/`. Source directories exist but are not modified by this plan.

## Phase 0: Research

Resolve dependency choices and validation pipeline decisions needed to implement deterministic, offline schema-first validation (including JSON Schema validator selection and failure-code determinism rules).

## Phase 1: Design & Contracts

- Define the validation result and failure-code data model.
- Define registry and pipeline contracts (schema-first, registry resolution, crypto verification, expiry classification).
- Publish contracts in `contracts/` and provide a quickstart.
- Update agent context via `.specify/scripts/bash/update-agent-context.sh copilot`.

## Phase 2: Planning

Implementation tasks are generated separately via `/speckit.tasks`.

## Post-Design Constitution Check

- Schema validation precedes proof verification: **PASS** (contracts + research).
- Deterministic failure for unknown identifiers: **PASS** (failure-code mapping contract).
- Result-object failures (no exceptions for expected failures): **PASS** (public API contract).
- No `proofBytes` logging: **PASS** (logging policy contract).
- Immutable DI registries: **PASS** (registry contract).
