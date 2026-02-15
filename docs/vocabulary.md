# Sigil — Vocabulary (v0.1)

This document defines the core terminology used throughout Sigil.

It is written for engineers and product designers who are new to:
- Software licensing infrastructure
- Zero-knowledge proofs (ZK)
- Privacy-preserving blockchains
- Offline license verification

If a term is used in specs or code, it should be defined here.

---

## Core Concepts

### License

A **license** is the right to use a piece of software under certain conditions.

In Sigil, a license is represented by:
- An **entitlement** (logical right to use)
- A corresponding **on-chain token** (source of truth)
- One or more **proofs** (installable license artifacts)

---

### Entitlement

An **entitlement** is the abstract permission granted to a customer.

Examples:
- Use Product X
- Enable "Pro" features
- Up to 10 seats
- Valid until 2026-12-31

An entitlement defines *what is allowed*.

---

### License Token (On-chain Asset)

A **License Token** is the private on-chain representation of an entitlement stored on the Midnight blockchain.

It:
- Is the authoritative source of license existence
- Is privacy-preserving
- Is not directly installed into software
- Is used to generate proofs

Think of it as the cryptographic anchor of the license.

---

### Proof

A **proof** is a zero-knowledge cryptographic artifact that demonstrates:

> "A valid license token exists that satisfies these conditions."

Without revealing:
- The full license details
- The owner's identity
- Sensitive internal data

This proof is what gets installed into software.

---

### Proof Envelope

A **Proof Envelope** is a portable, versioned container that transports a proof and its required metadata so it can be verified offline.

It is:

- The installable license artifact
- Delivered to customers (file, env var, secret, config)
- Verified locally using the Sigil SDK
- Self-contained and versioned

It is **not**:
- The license itself
- The blockchain token
- A simple signed key

Think of it as:

> A license file that contains a zero-knowledge proof instead of a static key.

---

### Zero-Knowledge Proof (ZK Proof)

A cryptographic proof that demonstrates a statement is true without revealing the underlying data.

In Sigil’s context:

It proves that:
- A valid license exists
- It satisfies specific constraints (expiry, features, etc.)

Without revealing:
- Private on-chain state
- Customer identity
- Sensitive license internals

---

### Midnight

Midnight is a privacy-preserving blockchain used as the durable storage layer for license tokens.

It:
- Stores entitlement state privately
- Enables generation of cryptographic proofs
- Acts as the source of truth for license existence

Sigil abstracts Midnight complexity from vendors.

---

### Policy

A **policy** defines the rules and constraints of a license.

Examples:
- Expiry date
- Maximum seats
- Enabled features
- Device binding requirement
- Version restrictions

A policy answers:

> What is this license allowed to do?

In Sigil, parts of the policy must be cryptographically bound to the proof so they can be enforced offline.

---

### Claim

A **claim** is a verifiable piece of information extracted from a validated proof.

Examples:
- "Edition = Pro"
- "MaxSeats = 10"
- "ExpiresAt = 2026-12-31"

Claims are what the application-under-license uses to enable or disable features.

---

### Public Inputs

In zero-knowledge systems, **public inputs** are values that are:
- Visible during verification
- Cryptographically bound to the proof

In Sigil, public inputs often contain the claims the application relies on.

---

### Circuit

A **circuit** is the cryptographic program that defines:

- What is being proven
- What constraints must hold
- What public inputs are exposed

Changing the circuit changes what the proof guarantees.

---

### Proof System

The cryptographic system used to generate and verify proofs.

Examples in the broader ecosystem include:
- Groth16
- PLONK
- Halo2

Sigil’s SDK must know which proof system is used in order to verify the proof.

---

### Verification

The act of validating a Proof Envelope locally inside an application using the Sigil SDK.

Verification checks:
- Envelope structure
- Proof validity
- Circuit compatibility
- Policy constraints encoded as public inputs

Verification does NOT require:
- Internet access
- Contacting Sigil Platform

---

### Offline Verification

License validation performed entirely within the application runtime, without calling external services.

This is a core Sigil design goal.

---

### Revocation

The act of invalidating a license so that future proofs fail validation.

Revocation can be:
- Immediate (new proofs fail)
- Time-bound
- Dependent on proof rotation

---

### Proof Rotation

The process of generating a new proof for the same entitlement, potentially invalidating older proofs.

Used for:
- Replay resistance
- Security updates
- Policy updates

---

### Device Binding

A constraint requiring that a proof be valid only for a specific machine or environment.

Typically achieved by:
- Binding proof generation to a device fingerprint
- Including device-specific inputs in the proof statement

---

### Envelope Version

The version of the Proof Envelope format.

It governs:
- JSON structure
- Required fields
- Encoding rules

Envelope versioning is independent from circuit versioning.

---

### Circuit Version

The version of the cryptographic circuit.

It governs:
- What is being proven
- What constraints are included
- What public inputs exist

Circuit versioning affects proof semantics.

---

### Tenant / Organization

A vendor account within Sigil Platform.

Each organization:
- Defines products
- Issues licenses
- Manages policies

Sigil is multi-tenant: multiple vendors operate independently on the same platform.

---

### Environment

A logical partition within an organization (e.g., Dev, Staging, Prod).

Each environment may:
- Use different cryptographic keys
- Issue different proof types
- Have different policy defaults

---

## Guiding Principle

If a term is ambiguous in a spec or discussion, update this document.

Infrastructure succeeds when vocabulary is precise.
