# Sigil — Product Vision (v0.1)

## One-liner
Sigil is a multi-tenant licensing platform that lets software vendors issue privacy-preserving, offline-verifiable licenses using zero-knowledge proofs backed by the Midnight blockchain.

## Vision
Make software licensing cryptographically verifiable, privacy-preserving, and resilient—without centralized license servers.

## Mission
Provide vendors with infrastructure to mint license entitlements as private on-chain assets and deliver installable license proofs that can be verified locally (offline) inside customer environments.

## The Problem
Traditional licensing systems:
- Rely on centralized license servers (downtime risk, operational burden, single point of failure).
- Leak customer or usage metadata via online checks.
- Are fragile under enterprise network constraints (airgapped / locked-down environments).
- Use license keys that are copied, shared, and hard to revoke safely.
- Make “self-hosted” distribution painful for SaaS vendors.

## The Sigil Approach
Sigil replaces “license keys” with **proofs**:
- A license entitlement exists as a private on-chain token (Midnight).
- The customer installs software with a **ZK proof**, not the entitlement itself.
- Software verifies the proof locally via Sigil’s open-source SDK.
- Vendors can manage lifecycle (issue, renew, upgrade, revoke) through Sigil.

## Target Customers
Primary:
- B2B software vendors distributing self-hosted products (Docker images, installers, on-prem).
- DevTools vendors (.NET ecosystem as first-class beachhead).

Secondary:
- Commercial OSS vendors.
- Enterprise ISVs with strict privacy/compliance requirements.
- Platform vendors distributing modular add-ons / feature packs.

## Core Value Propositions
For vendors:
- No license server to run.
- Stronger control over entitlements and lifecycle.
- Reduced fraud and key-sharing.
- A clean “issue → prove → verify” pipeline with auditability.

For end-customers:
- Offline verification (works in locked-down networks).
- Privacy-preserving: verification doesn’t phone home.
- Optional BYO wallet custody model.

## Differentiators
- ZK proof as install key (not a signed JWT, not a static key).
- Midnight-native private entitlements.
- Open-source verification SDK (trust + adoption).
- Strong multi-tenant platform design for vendors.

## Non-goals (v0.1)
- Becoming a general-purpose payments or subscription platform (we integrate with Stripe/etc.).
- Supporting every runtime/language from day one (start with .NET).
- On-chain public usage analytics (privacy is foundational).
- Building a “web3 wallet UX” as the main product (hide blockchain complexity).

## Key Principles
- SDK-first: verification must be dead simple.
- Privacy-by-default: no verification callouts required.
- Minimal surface area: small number of concepts, easy onboarding.
- Deterministic, spec-driven formats (proof envelope, schemas, versioning).
- Multi-tenant safety: isolation is a first-order requirement.

## Success Metrics
- Time-to-first-verify: < 10 minutes from install to successful local validation.
- Vendor onboarding: < 1 day to issue first real license to a customer.
- SDK adoption: third-party vendors embed Sigil SDK as default validation layer.
- Low operational burden: no vendor-managed verification infrastructure required.

## Monetization Thesis
Sigil Platform (closed source) monetizes via:
- Per active license / entitlement / proof generation volume.
- Vendor tiers (seats, environments, advanced policies).
- Enterprise: audit/compliance, advanced lifecycle controls, self-hosted issuer options.

Sigil SDK (.NET) is MIT to maximize trust and adoption.

## Near-term Strategy
Dogfood with your own distributed software (e.g., Docker-delivered products) to validate:
- UX flow
- Proof lifecycle
- Operational cost profile
- Edge cases (revocation, upgrades, device binding)

Then expand to early design partners in the .NET vendor space.
