# Contract — Expiry Evaluation Semantics (Spec 004)

## Purpose

Clarifies that expiry belongs to statement semantics conceptually, while execution remains a dedicated validator pipeline stage.

## Normative Behavior

1. Expiry MUST be evaluated only after statement validation succeeds.
2. Expiry input MUST come from validated claim value (`expiresAt`, unix seconds).
3. Comparison time source MUST be validator clock (UTC-based).
4. If `expiresAt` is in the past at evaluation time:
   - Status MUST be `Expired`.
   - Failure code MUST be deterministic and stable.
5. Malformed/invalid `publicInputs` MUST fail before expiry stage.

## Pipeline Position

- Read/parse -> Extract identifiers -> Schema validation -> Registry resolution -> Proof verification -> Statement validation -> Expiry evaluation

## Rationale

- Maintains deterministic, fail-closed behavior.
- Avoids duplicating expiry logic in every statement handler implementation.
