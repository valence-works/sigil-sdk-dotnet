# Research — Statement Handler Contract & license:v1 (Spec 004)

**Branch**: `004-statement-handler-contract`  
**Date**: 2026-02-19  
**Spec**: [specs/004-statement-handler-contract/spec.md](spec.md)

## Decision 1: Build/runtime baseline for this feature

**Decision**: Keep SDK target as `net8.0` and validate feature implementation with .NET 10 SDK toolchain.

**Rationale**:
- Maintains compatibility with existing SDK consumers.
- Satisfies current repository baseline while honoring the requirement to build with .NET 10.
- Avoids introducing unnecessary target-framework churn for a contract-focused feature.

**Alternatives considered**:
- Retarget SDK to `net10.0`: unnecessary for this feature scope and potentially breaking for consumers.
- Multi-target `net8.0` + `net10.0`: adds maintenance complexity without clear benefit here.

## Decision 2: Unknown `publicInputs` fields policy for `license:v1`

**Decision**: Use strict validation; reject unknown fields deterministically.

**Rationale**:
- Aligns with fail-closed behavior.
- Prevents accidental issuer-side schema drift.
- Improves predictability of statement semantics.

**Alternatives considered**:
- Ignore unknown fields for forward compatibility: more flexible, but can mask data quality issues.

## Decision 3: `issuedAt` claim extraction behavior

**Decision**: Extract `issuedAt` into claims when present.

**Rationale**:
- Preserves useful issuance metadata for integrators.
- Keeps claim mapping deterministic and explicit.

**Alternatives considered**:
- Never expose `issuedAt`: simpler but removes useful metadata.
- Configuration-gated extraction: unnecessary complexity for v1 statement definition.

## Decision 4: Cancellation behavior in statement handlers

**Decision**: Enforce cooperative cancellation semantics (`CancellationToken` honored, `OperationCanceledException` permitted).

**Rationale**:
- Standard .NET async contract behavior.
- Supports robust hosting scenarios and cancellation propagation.

**Alternatives considered**:
- Advisory token only: weakens contract consistency.
- Timeout-only policy: shifts concern away from established .NET cancellation model.

## Decision 5: Success path contract for claims

**Decision**: `IsValid=true` MUST include non-null claims; null claims on success are treated as handler error and fail validation.

**Rationale**:
- Tight contract prevents ambiguous success states.
- Preserves deterministic and fail-closed behavior.

**Alternatives considered**:
- Allow null claims on success: introduces ambiguous integration behavior and weakens contract guarantees.

## Decision 6: Expiry semantics ownership

**Decision**: Expiry is part of statement semantics conceptually; execution remains a dedicated validator pipeline stage.

**Rationale**:
- Matches existing pipeline architecture and performance profile.
- Keeps semantic ownership explicit in statement definition docs.

**Alternatives considered**:
- Move expiry execution fully inside statement handlers: increases per-handler duplication and weakens pipeline consistency.
