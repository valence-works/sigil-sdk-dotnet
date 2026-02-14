# Sigil — Domain Model (v0.1)

This document defines Sigil’s canonical vocabulary and core domain entities.
These terms must be used consistently across specs, code, API contracts, and docs.

## Core Concepts

### Tenant / Organization
A vendor account in Sigil Platform. The vendor uses Sigil to issue licenses for their own products.

### Environment
A logical partition within an Organization (e.g., Dev / Staging / Prod) with separate keys, configurations, and issuance settings.

### Product
A licensable software offering (SKU). Defines what can be purchased/entitled and what claims/entitlements it confers.

### License (Entitlement)
The abstract right to use a Product under certain constraints (time, seats, features, device binding, etc.).

### License Token (On-chain Asset)
A private on-chain representation of a License/Entitlement on Midnight.
- Stored privately (privacy-preserving).
- Acts as the “source of truth” for existence and state of the entitlement.
- Not directly installed into software.

### Proof (Installable License)
A zero-knowledge proof derived from the License Token + policy constraints + optional challenge inputs.
- This is what the customer installs into the software.
- Verified locally by Sigil SDK.

### Proof Envelope
A versioned, portable JSON container that transports proof bytes plus required metadata for validation.

### Policy
Rules governing issuance and validation.
Examples:
- expiration timestamp
- max seats
- feature flags
- device binding required/optional
- allowed product version range

### Verification
The act of validating a Proof Envelope locally in an application runtime using the Sigil SDK.

### Revocation
Invalidation of an entitlement such that future proofs (or existing proofs, depending on design) fail verification.

---

## Entities (Conceptual)

### Organization
- Id
- Name
- BillingAccountId
- Settings (wallet custody mode, default policies, etc.)

### Environment
- Id
- OrganizationId
- Name
- Issuer configuration (keys, endpoints, allowed circuits)
- Mode: Dev/Staging/Prod

### Product
- Id
- OrganizationId
- Name
- SKU
- DefaultPolicy
- Metadata (docs, marketing, internal tags)

### LicenseEntitlement
- Id
- OrganizationId
- ProductId
- CustomerReference (vendor-defined customer id)
- Status (Active, Suspended, Revoked, Expired)
- Terms (from policy snapshot at issuance)
- CreatedAt / UpdatedAt

### LicenseToken (Midnight)
- ChainId / Network
- AssetId (opaque identifier)
- Owner (wallet reference; customer-managed or vendor-managed custody)
- State commitments (private)
- MintedAt

### ProofEnvelope
- envelopeVersion
- proofSystem (identifier)
- circuitId (identifier)
- proofBytes (base64)
- publicInputs (structured; versioned)
- policySnapshot (hash or minimal representation)
- issuedAt
- expiresAt (optional)
- nonce/challenge (optional)
- licenseRef (opaque; optional)
- signature (optional; for envelope integrity)

> Note: The ProofEnvelope must be verifiable offline. Any fields that require online lookup must be optional.

### IssuanceRequest
- OrganizationId
- EnvironmentId
- ProductId
- CustomerReference
- PolicyOverrides (optional)
- DeliveryTarget (API response, webhook, email, etc.)

### IssuanceEvent (Audit)
- Type (Minted, ProofGenerated, Renewed, Upgraded, Revoked, etc.)
- Timestamp
- Actor (user/service)
- CorrelationId
- Details (redacted/structured)

---

## Key Relationships
- Organization 1—N Environment
- Organization 1—N Product
- Product 1—N LicenseEntitlement
- LicenseEntitlement 1—1..N LicenseToken (usually 1, but upgrades/renewals may create new token state)
- LicenseToken 1—N ProofEnvelope (proof rotation is possible)

---

## Core Flows (High Level)

### Issue License
1. Vendor creates Product + policy.
2. Vendor requests issuance for a customer.
3. Sigil mints a private License Token on Midnight.
4. Sigil generates a Proof Envelope for installation.
5. Vendor delivers proof to customer (direct download/API/webhook).

### Verify License (Offline)
1. Customer installs Proof Envelope in software.
2. Sigil SDK verifies proof locally.
3. App extracts entitlements/features from verified public inputs.
4. App enables features accordingly.

### Renew / Upgrade
- Update entitlement state and generate new proof envelope.
- Optionally rotate proof and invalidate old envelope (policy-driven).

### Revoke
- Mark entitlement as revoked and ensure future proofs fail (design-dependent).
