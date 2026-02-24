# Research — Proof System Verifier Contract (Spec 005)

**Branch**: `005-proof-system-verifier-contract`  
**Date**: 2026-02-20  
**Spec**: [specs/005-proof-system-verifier-contract/spec.md](spec.md)

## Decision 1: Verifier abstraction boundary

**Decision**: Define proof-system verifier contract to accept only `proofBytes` and statement-produced verification context, while excluding statement semantics.

**Rationale**:
- Enforces clear separation of concerns: proof systems define HOW verification is done, statements define WHAT is being proven.
- Preserves extensibility without coupling new proof systems to statement-specific domain meaning.
- Aligns with existing Spec 004 statement handler responsibilities.

**Alternatives considered**:
- Let verifier inspect statement semantics directly: rejected because it creates cross-layer coupling and ambiguous ownership.
- Push all verification context construction into verifiers: rejected because statement handlers already own statement-specific shaping.

## Decision 2: Identifier matching policy

**Decision**: Use exact, case-sensitive canonical `proofSystem` identifier matching with no normalization.

**Rationale**:
- Deterministic fail-closed behavior for unknown identifiers.
- Avoids alias/case-folding ambiguity.
- Maintains stable registry keys across environments.

**Alternatives considered**:
- Case-insensitive matching: rejected because it can mask issuer/integration defects.
- Alias maps: rejected for v1 due to added ambiguity and governance overhead.

## Decision 3: Cancellation behavior

**Decision**: Propagate cancellation to callers and do not map cancellation to validation statuses.

**Rationale**:
- Matches standard .NET cooperative cancellation contracts.
- Keeps validation statuses reserved for domain outcomes, not control-flow events.
- Preserves deterministic status taxonomy for non-cancelled runs.

**Alternatives considered**:
- Map cancellation to `Error`: rejected because it conflates caller intent with system faults.
- Ignore cancellation: rejected as non-cooperative and host-unfriendly.

## Decision 4: Failure mapping for proof/context incompatibility

**Decision**: For supported proof systems, proof/context incompatibility maps to `Invalid` with deterministic failure code.

**Rationale**:
- Represents expected verification failure, not infrastructure fault.
- Consistent with Spec 002 rule that proof verification failures classify as `Invalid`.
- Maintains predictable first-failing-stage behavior.

**Alternatives considered**:
- Map incompatibility to `Error`: rejected because input-level mismatch is not an internal runtime failure.
- Map incompatibility to `Unsupported`: rejected because proof system is already recognized.

## Decision 5: Contract artifact format

**Decision**: Publish an OpenAPI contract artifact for verification flow scenarios in `contracts/proof-system-verifier.openapi.yaml`.

**Rationale**:
- Provides machine-readable, testable contract shape for conformance and integration docs.
- Captures deterministic status/failure mappings and stage precedence in one artifact.
- Supports future cross-language client and contract-test generation.

**Alternatives considered**:
- Markdown-only contract: rejected for lower machine verifiability.
- GraphQL schema: rejected because verification flow is command-oriented and simpler in REST-style request/response modeling.

## Decision 6: Performance target interpretation

**Decision**: Keep Spec 005 performance target aligned with Spec 002: SDK validation p95 < 1s for envelopes <= 10 KB, with verifier-level guidance that initialization/caching may be used but not mandated.

**Rationale**:
- Maintains consistency with existing measurable outcomes.
- Avoids over-constraining implementation choices for future proof systems.
- Preserves technology-agnostic contract language.

**Alternatives considered**:
- Add strict per-verifier millisecond SLOs: rejected as too prescriptive for unknown future proof systems.
- Omit verifier performance target entirely: rejected because feature requires measurable non-functional criteria.
