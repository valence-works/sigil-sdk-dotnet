# Research — SDK Validation API (Spec 002)

**Branch**: `002-sdk-validation-api`  
**Date**: 2026-02-16  
**Spec**: [specs/002-sdk-validation-api/spec.md](spec.md)

## Decision 1: JSON Schema validator

**Decision**: Use **Corvus.Json.Validator** (Draft 2020-12 capable; `System.Text.Json`-compatible) and initialize the Spec 001 schema once at startup for reuse across validations.

**Rationale**:
- Spec 001’s schema is explicitly Draft 2020-12 and uses modern keywords (e.g., `$defs`, `const`, `contentEncoding`).
- Validation MUST be offline and deterministic; schema validation is a hard gate before cryptographic verification.
- The SDK should avoid dependencies that impose paid/EULA obligations on downstream consumers.

**Alternatives considered**:
- **JsonSchema.Net (json-everything)**: Best technical fit (System.Text.Json-first; modern drafts; strong evaluation report), but carries an “open source maintenance fee” EULA obligation risk for downstream users.
- **Newtonsoft.Json.Schema**: Strong Draft 2020-12 support and streaming validation, but licensing (commercial/AGPL) is not suitable for a broadly-redistributed MIT SDK.
- **NJsonSchema**: MIT and widely used, but is Json.NET-centric and draft-2020-12 fidelity must be verified carefully against Spec 001’s schema.

## Decision 2: Deterministic failure codes and status mapping

**Decision**: Treat failure codes as a stable, versioned public contract. Codes are deterministic by construction: the returned code is the first failing pipeline stage code.

**Rationale**:
- Callers must implement policy based on stable identifiers, not string messages.
- Determinism is enforced by a strict stage order and by forbidding code reuse.

**Alternatives considered**:
- Returning multiple codes / aggregated errors: provides more detail but complicates determinism and makes callers’ policy decisions ambiguous unless ordering rules are specified.

## Decision 3: Validation pipeline order (fail-closed)

**Decision**: Use a strict stage order:
1. Read input (string/stream) and parse JSON.
2. Best-effort extract `envelopeVersion`, `proofSystem`, `statementId` (for routing/log correlation) without trusting claims.
3. Validate Spec 001 JSON schema.
4. Resolve `proofSystem` and `statementId` using immutable DI registries.
5. Cryptographic verification.
6. Semantic checks based on verified claims, including expiry (`publicInputs.expiresAt`).

**Rationale**:
- Satisfies constitution: schema-first and deterministic routing.
- “Fail closed”: any stage failure is non-`Valid`.
- Avoids trusting attacker-controlled expiry before proof verification.

**Alternatives considered**:
- Checking `expiresAt` before crypto verification: faster but risks attacker-controlled timestamps influencing status classification.

## Decision 4: Structured logging and diagnostics

**Decision**: Structured logs only; never log `proofBytes` (or any derivative), and keep diagnostics opt-in and separate from log level.

**Rationale**:
- Proof confidentiality is a constitution gate.
- Diagnostics must not change validation outcomes.

**Alternatives considered**:
- Relying on redaction of `proofBytes`: rejected; safest approach is to never pass sensitive values into logging APIs.

## Decision 5: Time source for expiry

**Decision**: Treat “current time” as UTC and design expiry evaluation around an injectable time source for testability (while remaining deterministic per input + time).

**Rationale**:
- Expiry depends on current time; tests need a stable clock.
- UTC avoids local-time ambiguity.

**Alternatives considered**:
- Using `DateTime.UtcNow` directly everywhere: simplest, but makes deterministic testing harder.
