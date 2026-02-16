# Sigil SDK — Specifications

Sigil SDK is specification-driven infrastructure.

No production code changes may be introduced without a corresponding spec in `/specs`.

---

## Spec Lifecycle

1. Draft
2. Review
3. Accepted
4. Implemented
5. Deprecated (if needed)

A spec MUST reach **Accepted** status before implementation begins.

Implementation PRs MUST reference the spec number (e.g., `Implements Spec 002`).

---

## Required Sections in Every Spec

Each spec MUST define:

- Problem
- Goals
- Non-Goals
- Requirements (Functional & Non-Functional)
- Public API impact
- Versioning implications
- Backward compatibility impact
- Security considerations
- Acceptance criteria

---

## Backward Compatibility Rules

Sigil SDK is infrastructure. Backward compatibility is a first-class concern.

Specs MUST explicitly state:

- Whether the change is backward compatible.
- Whether it introduces a new:
  - envelope version
  - statementId
  - proofSystem
- Migration strategy (if applicable).

Breaking changes require:
- Major version bump (SDK and/or envelope).
- Explicit justification in the spec.

---

## Registry Discipline

Sigil SDK maintains registries for:

- Supported `proofSystem` identifiers.
- Supported `statementId` identifiers.

Specs that introduce new identifiers MUST:

- Define expected behavior for unsupported identifiers.
- Specify failure modes.
- Describe deprecation strategy (if replacing an existing identifier).

Unknown identifiers MUST fail validation deterministically.

---

## Evolution Discipline

- Additive changes are preferred over breaking changes.
- New semantic behavior MUST use a new `statementId`.
- Transport changes MUST use a new `envelopeVersion`.
- Proof mechanism changes MUST use a new `proofSystem`.

---

## Security Review Requirement

Any spec that affects:

- Cryptographic validation
- Claim interpretation
- License enforcement semantics

MUST include a Security Considerations section and threat model analysis.

---

## Design Principle

Sigil SDK must:

- Fail closed.
- Be deterministic.
- Remain proof-system agnostic.
- Avoid coupling transport structure to semantic meaning.

---

## Specs Index

- **Spec 001**: Proof Envelope Format (v1.0) — `specs/001-proof-envelope-format/`
