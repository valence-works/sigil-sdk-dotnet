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
   - expiry evaluation (conceptually part of statement semantics, executed after statement validation using validated claims)

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

## Dependency Injection Integration (Spec 003)

### Design Philosophy

The SDK provides a **drop-in validator** pattern for .NET applications using the `AddSigilValidation()` extension method. Integration requires 3-5 lines of setup code with zero configuration for standard use cases.

### Service Registration Strategy

All services are registered as **singletons** for optimal performance and thread-safety:

| Service | Interface | Lifetime | Rationale |
|---------|-----------|----------|-----------|
| Validator | `ILicenseValidator` | Singleton | Stateless, thread-safe, expensive to initialize |
| Proof System Registry | `IProofSystemRegistry` | Singleton | Immutable after registration |
| Statement Registry | `IStatementRegistry` | Singleton | Immutable after registration |
| Schema Validator | `IProofEnvelopeSchemaValidator` | Singleton | Compiled schema, thread-safe |
| Clock | `IClock` | Singleton | Stateless time abstraction |

**Key Decision**: Singleton lifetime ensures:
- Schema compilation happens once at startup (< 100ms)
- Memory overhead remains minimal (< 5MB)
- No service resolution overhead per validation call
- Thread-safe concurrent validation

### Extension Points

The SDK provides two extension points for custom integrations:

1. **Custom Proof Systems** (`options.AddProofSystem(id, verifier)`)
   - Developers register custom zero-knowledge proof verifiers
   - Verifiers are added to immutable registry during service registration
   - Duplicate identifiers throw `ArgumentException` immediately

2. **Custom Statement Handlers** (`options.AddStatementHandler(handler)`)
   - Developers register custom validation logic for application-specific statements
   - Handlers are added to immutable registry during service registration
   - Handlers expose a canonical `StatementId` (URN) used as the registry key
   - Duplicate statement IDs throw `ArgumentException` immediately

**Immutability Enforcement**: Registries are sealed after `BuildServiceProvider()` is called. Any attempt to modify registries at runtime throws `InvalidOperationException` with clear guidance to use `ValidationOptions` during registration (Spec 003 FR-015).

### Configuration Options

`ValidationOptions` provides two diagnostic configuration flags:

1. **`EnableDiagnostics`** (default: `false`)
   - Controls whether diagnostic information is collected during validation
   - Minimal performance impact when enabled

2. **`LogFailureDetails`** (default: `false`)
   - Controls whether detailed failure information appears in logs
   - **Security-sensitive**: Should only be enabled in secured environments
   - Defaults to secure mode (status + failure code only)

**Default Configuration**: Production-ready with secure defaults. No sensitive data logging, optimal performance, zero required configuration.

### Error Handling Strategy

The SDK follows a **fail-fast** approach for configuration errors:

- Duplicate registrations throw `InvalidOperationException` immediately
- Invalid identifiers (empty/whitespace) throw `ArgumentException` immediately
- Missing dependencies cause `InvalidOperationException` during container build
- Runtime modification attempts throw `InvalidOperationException` with clear remediation guidance

**Design Goal**: Developers discover configuration errors at startup, not during production validation calls.

### Performance Characteristics

- **Startup Time**: Schema compilation < 100ms (Spec 003 SC-002)
- **Memory Overhead**: Default configuration < 5MB (Spec 003 SC-004)
- **Service Resolution**: O(1) singleton resolution per injection point
- **Validation Latency**: P95 < 1 second for 10KB envelopes (Spec 002 SC-004)

### Integration Scenarios

The SDK supports multiple .NET hosting models:

1. **ASP.NET Core Web API**
   ```csharp
   builder.Services.AddSigilValidation();
   // Inject ILicenseValidator into controllers
   ```

2. **Console Application**
   ```csharp
   var services = new ServiceCollection();
   services.AddSigilValidation();
   var provider = services.BuildServiceProvider();
   ```

3. **Worker Service / Background Processing**
   ```csharp
   Host.CreateDefaultBuilder(args)
       .ConfigureServices(services => services.AddSigilValidation())
       .Build();
   ```

All scenarios use identical registration pattern with optional configuration lambda.

### Security Considerations

1. **No Proof Bytes in Logs**: SDK enforces constitution constraint that `proofBytes` never appear in logs or error messages
2. **Secure Defaults**: `LogFailureDetails` defaults to `false` to prevent information leakage
3. **Immutable Registries**: Prevents runtime tampering with proof system or statement validation logic
4. **Deterministic Validation**: All validation logic is deterministic and side-effect free

See [DI Integration Guide](DI_INTEGRATION.md) for complete usage documentation.

---

## Future Extensions
- Proof rotation
- Revocation proofs / state updates
- Device binding
- Policies as code
- Self-hosted issuer components for enterprise
