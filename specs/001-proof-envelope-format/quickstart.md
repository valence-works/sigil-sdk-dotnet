# Quickstart: Proof Envelope Format

This guide shows the minimal JSON required to produce a valid Proof Envelope Format v1.0 document for offline verification.

## Minimal Envelope

```json
{
  "envelopeVersion": "1.0",
  "proofSystem": "sigil:example-system",
  "statementId": "urn:sigil:statement:example-v1",
  "proofBytes": "AAECAw==",
  "publicInputs": {
    "productId": "sigil-starter",
    "edition": "community",
    "features": ["basic-access"]
  }
}
```

## Full Envelope

```json
{
  "envelopeVersion": "1.0",
  "proofSystem": "sigil:example-system",
  "statementId": "urn:sigil:statement:example-v1",
  "proofBytes": "AAECAw==",
  "publicInputs": {
    "productId": "sigil-starter",
    "edition": "community",
    "features": ["basic-access", "offline-verify"],
    "expiresAt": "2030-01-01T00:00:00Z",
    "maxSeats": 25
  },
  "issuedAt": "2026-02-15T12:00:00Z",
  "policyHash": "policy-2026-02-15",
  "extensions": {
    "note": "example metadata"
  }
}
```

## Validation Notes

- Validation is offline-only and fail-closed.
- Unknown top-level fields are ignored; unknown `publicInputs` fields are rejected.
- Unknown `proofSystem` or `statementId` must fail deterministically with a stable failure category.
- `expiresAt` is compared against current UTC time; expired envelopes fail with `Expired`.
- `proofBytes` must never be logged or emitted in diagnostics.
