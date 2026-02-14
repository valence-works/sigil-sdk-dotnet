# Sigil — Architecture (v0.1)

Sigil consists of:
- Sigil Platform (closed source SaaS): multi-tenant issuance + lifecycle management
- Sigil SDK (.NET, MIT): offline proof verification library used inside customer software
- Midnight blockchain: private on-chain source of truth for entitlements
- Optional: self-hosted validator/issuer components for enterprise later

## Architectural Goals
- Offline verification: customer apps validate proofs without calling Sigil Platform.
- Privacy-first: no leakage of customer identity/usage during verification.
- Multi-tenant isolation: strong separation between vendor tenants.
- Minimal client complexity: SDK integration must be simple.
- Spec-driven formats: envelope schema and verification behavior are deterministic and versioned.

---

## Component Overview

### 1) Sigil Platform (SaaS)
Responsibilities:
- Vendor onboarding (orgs, envs, users, RBAC)
- Product/SKU management
- Policy management
- Issuance workflow orchestration
- Midnight interactions (mint/updates)
- Proof generation service
- Delivery mechanisms (API, webhooks, download links)
- Audit logging + observability
- Billing integration

Non-responsibilities (by design):
- Runtime verification of customer installations
- Storing customer usage telemetry by default

### 2) Sigil SDK (.NET, MIT)
Responsibilities:
- Validate Proof Envelope locally
- Provide stable API to applications:
  - `ValidateAsync(envelope, options)` -> result (valid/invalid + extracted claims)
- Envelope parsing + schema validation
- Versioning support
- Optional: ASP.NET Core middleware + DI extensions
- Optional: CLI tool for debugging/validation

Non-responsibilities:
- Proof generation
- License lifecycle management
- Customer identity management

### 3) Midnight Chain
Responsibilities:
- Host private entitlement state (license token)
- Provide the cryptographic substrate for proof generation and verification

Sigil Platform encapsulates Midnight complexity so vendors don’t have to.

---

## Trust Boundaries
- Customer runtime trusts Sigil SDK verification result, not Sigil Platform availability.
- Sigil Platform is trusted for issuance, but not required at runtime.
- Midnight is the durability/authority layer for entitlement existence and lifecycle state.

---

## Custody Modes (v0.1)
Support these conceptually; implement minimal first.

1) Vendor-managed custody (default MVP)
- Sigil Platform manages wallets on behalf of vendor/customers.
- Simplifies onboarding massively.

2) Customer-managed custody (later)
- Customer provides wallet reference / signs transactions.
- Higher trust, higher complexity.

---

## Issuance Architecture (Logical)
1. Platform receives IssuanceRequest.
2. Platform mints or updates a LicenseToken on Midnight.
3. Platform generates ProofEnvelope (Proof Generation Service).
4. Platform returns ProofEnvelope to vendor (API) and/or triggers delivery webhook.

Key services:
- TenantService
- ProductService
- PolicyService
- IssuanceService
- MidnightAdapter
- ProofGenerator
- AuditLogService
- BillingService

---

## Verification Architecture (Logical)
- Application embeds Sigil SDK (.NET).
- SDK validates envelope:
  - schema correctness
  - proof cryptographic validity
  - policy constraints encoded as public inputs
  - optional challenge/nonce rules

Return:
- validity
- reason codes
- claims/features (public inputs)
- expiry info

---

## Versioning Strategy
- ProofEnvelope has `envelopeVersion`.
- Proof system/circuit identified via `proofSystem` + `circuitId`.
- SDK supports multiple envelope versions and circuits through strategy/registry pattern.

Rules:
- Never break verification semantics for an existing `envelopeVersion` without a major version bump.
- Additive fields in envelope must be backwards compatible.
- Deprecation policy defined via ADRs.

---

## Security Considerations (v0.1)
- Envelope integrity: consider signing the envelope to prevent tampering even if proof remains valid.
- Replay resistance: optional nonce/challenge mechanism (depends on customer UX).
- Key separation per environment (dev/staging/prod).
- Strict tenant isolation at data and crypto levels.
- Audit log immutability (enterprise-grade later).

---

## Minimal MVP Architecture
MVP aims for the smallest set that proves the model:

Platform:
- Org + Environment
- Product + basic policy
- Issue license -> mint token -> generate proof -> return proof
- Stripe: optional; can be stubbed initially

SDK:
- Envelope parsing + validation API
- Proof verification integration (WASM or native binding depending on Midnight tooling)
- Sample app + docs

---

## Future Extensions
- Proof rotation
- Revocation proofs / state updates
- Device binding
- Policies as code
- Self-hosted issuer components for enterprise
