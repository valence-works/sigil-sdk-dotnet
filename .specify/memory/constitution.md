<!--
Sync Impact Report
- Version change: N/A (template) -> 0.1.0
- Modified principles: N/A (template placeholders)
- Added sections: Core Principles; Additional Constraints; Development Workflow
- Removed sections: None
- Templates requiring updates:
	- ✅ .specify/templates/plan-template.md
	- ✅ .specify/templates/spec-template.md
	- ✅ .specify/templates/tasks-template.md
	- ⚠️ .specify/templates/commands/*.md (directory not present)
- Follow-up TODOs:
	- TODO(RATIFICATION_DATE): original ratification date not found in repo
-->
# Sigil SDK (.NET) Constitution

## Core Principles

### I. Spec-Driven, Deterministic Validation
All validation MUST be driven by published specs and schemas. Schema validation
MUST run before proof verification. Unknown `proofSystem` or `statementId` MUST
fail deterministically. Validation logic MUST be deterministic and side-effect
free and MUST avoid platform-specific dependencies in core validation paths.
Rationale: deterministic, spec-anchored behavior ensures offline verification is
reliable and auditable across environments.

### II. Result-Based Validation Errors
Validation failures MUST return a result object that reports errors without
throwing. Exceptions are reserved for programmer errors (e.g., null arguments or
misconfiguration), not for invalid inputs or failed checks. Rationale: consumers
need predictable, non-throwing validation APIs for offline license enforcement.

### III. Proof Confidentiality
`proofBytes` MUST never be logged, traced, or emitted in diagnostics. Redact or
omit proof material from all logs and error messages. Rationale: proof material
is sensitive and must not leak through observability channels.

### IV. Immutable, DI-Configured Registries
Proof-system and statement registries MUST be provided via dependency injection
and MUST be immutable after construction. Runtime mutation is prohibited.
Rationale: immutability prevents nondeterministic validation behavior.

### V. Controlled Breaking Changes
Breaking changes require an ADR or spec update with explicit versioning notes
and a migration plan. No breaking change is permitted without documentation.
Rationale: SDK consumers need stable contracts and predictable upgrades.

## Additional Constraints

- Core validation MUST be offline-capable and MUST NOT perform network calls.
- Validation outputs MUST be deterministic for identical inputs.
- Schema and verification order is non-negotiable: schema first, proof second.

## Development Workflow

- Production code MUST NOT be introduced without a corresponding spec in
	`/specs`.
- Plans and specs MUST include a constitution check and list any violations.
- PR reviews MUST confirm compliance with all Core Principles.

## Governance

- Amendments require a PR that updates this constitution and documents the
	rationale in an ADR or spec where applicable.
- Versioning uses semantic versioning: MAJOR for breaking governance changes,
	MINOR for new principles or material expansions, PATCH for clarifications.
- Compliance is reviewed in plans, specs, and code reviews before merge.

**Version**: 0.1.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption
date not found | **Last Amended**: 2026-02-15
