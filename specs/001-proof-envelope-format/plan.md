# Implementation Plan: Proof Envelope Format

**Branch**: `001-proof-envelope-format` | **Date**: 2026-02-15 | **Spec**: [specs/001-proof-envelope-format/spec.md](specs/001-proof-envelope-format/spec.md)
**Input**: Feature specification from /specs/001-proof-envelope-format/spec.md

## Summary

Define a versioned, offline-verifiable JSON Proof Envelope format with strict `publicInputs`, forward-compatible top-level fields, deterministic failure conditions, and explicit security constraints. Deliverables include a formal JSON Schema contract, data model documentation, and a quickstart usage guide.

## Technical Context

**Language/Version**: C# / .NET 8 (planned SDK target; repository currently empty)  
**Primary Dependencies**: System.Text.Json; JSON Schema validator (e.g., JsonSchema.Net)  
**Storage**: N/A  
**Testing**: xUnit (planned)  
**Target Platform**: Cross-platform .NET 8 (Windows/macOS/Linux)  
**Project Type**: Single SDK library  
**Performance Goals**: Validate <=10 KB envelopes within 1 second offline (per SC-003)  
**Constraints**: Offline-only validation, deterministic failures, fail-closed, no `proofBytes` logging  
**Scale/Scope**: Single envelope format v1.0 with strict `publicInputs`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Schema validation MUST run before proof verification. **PASS** (captured in spec).
- Unknown `proofSystem` or `statementId` MUST fail deterministically. **PASS** (FR-012).
- Validation failures MUST return a result object (no throws). **N/A** (format-only feature; enforcement in validation API spec).
- `proofBytes` MUST NOT be logged or emitted in diagnostics. **PASS** (FR-015).
- Registries MUST be DI-configured and immutable after construction. **N/A** (format-only feature).
- Breaking changes require an ADR/spec with explicit versioning notes. **PASS** (no breaking change proposed).

## Project Structure

### Documentation (this feature)

```text
specs/001-proof-envelope-format/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── proof-envelope.schema.json
└── tasks.md
```

### Source Code (repository root)

```text
src/

tests/
```

**Structure Decision**: Documentation-only changes for this feature; source code directories exist but are not modified by this plan.

## Phase 0: Research

Resolve format constraints and schema details to remove ambiguity in the envelope contract.

## Phase 1: Design & Contracts

- Define data model for `ProofEnvelope` and `PublicInputs`.
- Publish JSON Schema contract for v1.0 in `contracts/`.
- Provide quickstart guidance and example payloads.
- Update agent context via `.specify/scripts/bash/update-agent-context.sh copilot` (see T019).

## Phase 2: Planning

No production code tasks in this plan; implementation tasks deferred to feature-specific SDK validation work.

## Post-Design Constitution Check

- Schema validation precedes proof verification: **PASS** (documented in contract guidance).
- Deterministic failure for unknown identifiers: **PASS** (spec requirement FR-012).
- No `proofBytes` logging: **PASS** (FR-015 and quickstart notes).
- Breaking changes documented: **PASS** (versioned envelope set to 1.0).
