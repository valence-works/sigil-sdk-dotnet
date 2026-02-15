# Spec 001 — Proof Envelope

## Status

Accepted

## Problem

Sigil requires a portable, offline-installable artifact that customers can provide to an application in order to prove they have a valid license entitlement. This artifact must:

- Be verifiable locally (offline), without contacting Sigil Platform.
- Be safe to parse and validate deterministically.
- Carry a small set of license claims that the application-under-license can use to enable/disable features.
- Support long-term evolution without breaking existing deployed software.

## Goals

- Define a **versioned JSON format** for distributing license proofs.
- Ensure the envelope contains all information required for **offline verification**.
- Provide a **fixed v0.1 claim schema** that applications can rely on:
  - product identity
  - edition
  - features
  - expiry
  - max seats
- Separate **envelope versioning** (transport/schema) from **statement versioning** (proof semantics).
- Remain proof-system agnostic.

## Non-Goals (v0.1)

- Device binding
- Challenge/nonce-based replay protection
- Envelope signing / provenance signatures
- Online verification or calling Sigil Platform
- Revocation semantics and proof rotation rules
- Vendor-defined custom claims (planned for a later revision)

---

## Requirements

### Functional Requirements

1. The envelope MUST be parseable as JSON.
2. The envelope MUST declare an `envelopeVersion` that determines parsing and validation rules.
3. The envelope MUST include identifiers for:
   - proof system (verification mechanism)
   - statement (semantic contract of the proof)
4. The envelope MUST contain a base64-encoded proof payload (`proofBytes`).
5. The envelope MUST carry the fixed claim set in `publicInputs` and those claims MUST be cryptographically bound to the proof via the statement definition.
6. The envelope SHOULD include `issuedAt`.
7. The envelope MAY include `policyHash` for traceability.

### Non-Functional Requirements

- Offline-first: No required external lookup is allowed during verification.
- Forward-compatible parsing: Unknown fields MUST be ignored by verifiers.
- Deterministic behavior: Given the same envelope, validation results MUST be stable.
- Minimal size: The envelope SHOULD remain reasonably small to allow usage in files, environment variables, and secrets.

---

## Envelope Format (v1.0)

### Top-Level Fields

- `envelopeVersion` (string, required)
  - Version of the envelope schema/format.
  - v0.1 defines `1.0`.

- `proofSystem` (string, required)
  - Identifier of the verification mechanism and parameters required to validate `proofBytes`.
  - Defines **how** the proof is verified.
  - Does NOT define the semantic meaning of claims.
  - Example: `midnight-zk-v1`

- `statementId` (string, required)
  - Identifier of the semantic contract of the proof.
  - Defines **what** is being proven and how public inputs are interpreted.
  - MUST be a URI/URN string.
  - RECOMMENDED format: `urn:sigil:statement:<name>:v<major>`
  - Example: `urn:sigil:statement:license:v1`

- `proofBytes` (string, required)
  - Base64-encoded proof bytes.

- `publicInputs` (object, required)
  - Fixed v0.1 claim schema.
  - MUST match the statement definition referenced by `statementId`.

- `issuedAt` (string, optional)
  - ISO 8601 timestamp.

- `policyHash` (string, optional)
  - A stable identifier or hash referencing the vendor-side policy snapshot used for issuance.
  - Informational in v0.1 (no lookup required during verification).

- `extensions` (object, optional)
  - Reserved for forward-compatible additions and future vendor-defined claims.
  - Verifiers MUST ignore unknown fields under `extensions` in v0.1.

---

## Fixed Claim Schema (publicInputs) — v0.1

`publicInputs` MUST contain:

- `productId` (string, required)
  - Stable vendor-defined product/SKU identifier.
  - SHOULD remain stable across environments (dev/staging/prod).
  - SHOULD NOT be a database primary key.
  - Recommended format: `vendor:product` or similar namespaced string.
  - Example: `acme:reporting-suite`

- `edition` (string, required)
  - Example: `community`, `pro`, `enterprise`.

- `features` (array of string, required)
  - List of enabled feature identifiers.
  - Empty list allowed.

- `expiresAt` (string, optional)
  - ISO 8601 timestamp.
  - If omitted, license is considered non-expiring (per statement semantics).

- `maxSeats` (integer, optional)
  - Maximum allowed seats/users.
  - If omitted, seat count is unlimited (per statement semantics).

---

## Claim Semantics

- Applications MUST treat claim values as trustworthy ONLY if proof verification succeeds.
- The verifier MUST return these values as validated claims after successful verification.
- Applications SHOULD consume claims from the SDK validation result rather than parsing the envelope directly.
- The meaning of claims is governed by the `statementId`.

---

## Versioning

### Envelope Versioning

- `envelopeVersion` controls JSON structure and parsing rules.
- v0.1 defines `1.0`.
- Backwards compatibility rules:
  - Additive changes (new optional fields) are allowed.
  - Introducing new required fields requires a new major envelope version.
  - Parsers MUST ignore unknown fields to support forward compatibility.

### Statement Versioning

- `statementId` controls proof semantics and meaning of public inputs.
- Semantic changes (new constraints, new public inputs, changed interpretation) MUST use a new `statementId`
  (e.g., `urn:sigil:statement:license:v2`).
- Envelope version and statement version are independent.

### Proof System Evolution

- `proofSystem` identifies the verification backend and parameters.
- Changes to the underlying proof mechanism (e.g., new proving system, new verifier parameters) MUST use a new `proofSystem` identifier.
- `proofSystem` does NOT imply semantic changes to claims.

---

## Security Considerations (v0.1)

- The envelope contains claims that influence application behavior (feature gating). These claims MUST be cryptographically bound to the proof via the statement definition.
- Envelope integrity is not separately signed in v0.1. If any fields outside statement-bound public inputs are later used for decisions, envelope signing MUST be introduced.
- Proofs are copyable in v0.1. Device binding and replay resistance are deferred to future specifications.
- Applications MUST fail closed (treat license as invalid) if verification fails for any reason.

---

## Example Envelope (Informative)

```json
{
  "envelopeVersion": "1.0",
  "proofSystem": "midnight-zk-v1",
  "statementId": "urn:sigil:statement:license:v1",
  "proofBytes": "BASE64_PROOF_BYTES",
  "publicInputs": {
    "productId": "acme:reporting-suite",
    "edition": "enterprise",
    "features": ["audit", "multitenant"],
    "expiresAt": "2026-12-31T23:59:59Z",
    "maxSeats": 10
  },
  "issuedAt": "2026-02-14T12:00:00Z",
  "policyHash": "SHA256:abcdef...",
  "extensions": {}
}
````

---

## Acceptance Criteria

* A JSON Schema can be produced that validates the envelope format for v1.0.
* The Sigil SDK can parse an envelope and expose:

  * envelope version
  * proof system
  * statement id
  * validated claims (publicInputs)
* The Sigil SDK validates the envelope schema before attempting proof verification.
* Unknown fields do not cause parsing failures (forward compatible).
* A reference test vector (example envelope + expected validation result) can be produced.