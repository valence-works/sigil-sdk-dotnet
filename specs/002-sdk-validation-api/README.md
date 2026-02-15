# Spec 002 — SDK Validation API

## Status

Draft

---

## Problem

Applications embedding Sigil SDK must be able to:

- Validate a Proof Envelope.
- Receive deterministic validation results.
- Access strongly-typed license claims.
- Handle validation failures without crashing.
- Remain compatible with future statements and proof systems.

The SDK must provide a clean API that:

- Fails closed.
- Avoids throwing for normal validation failures.
- Supports extensibility via statement and proof-system registries.
- Maintains strict compatibility discipline.

---

## Goals

- Provide a result-object validation API (no exceptions for normal failures).
- Support validation from JSON string and Stream.
- Enforce registry-based resolution of `proofSystem` and `statementId`.
- Provide strongly-typed claims for v1 statements.
- Allow future statement extensibility without breaking callers.
- Keep ProofEnvelope parsing internal for v1.
- Provide a coarse-grained status model for common branching.

---

## Non-Goals (v1)

- Automatic remote fetching of proofs.
- License auto-renewal.
- Device binding enforcement.
- Dynamic schema loading.
- Plugin discovery via reflection (manual registration only).

---

## Public API Surface

### Primary Entry Points

```csharp
Task<LicenseValidationResult> ValidateAsync(
    string envelopeJson,
    CancellationToken cancellationToken = default);

Task<LicenseValidationResult> ValidateAsync(
    Stream envelopeStream,
    CancellationToken cancellationToken = default);
````

---

## Result Model

```csharp
public enum LicenseStatus
{
    Valid,
    Invalid,
    Expired,
    Unsupported,
    Malformed,
    Error
}

public sealed class LicenseValidationResult
{
    public LicenseStatus Status { get; }

    public bool IsValid { get; }

    public string? EnvelopeVersion { get; }

    public string? StatementId { get; }
    public string? ProofSystem { get; }

    public LicenseClaims? Claims { get; }

    public LicenseValidationFailure? Failure { get; }
}
```

* `IsValid` MUST be `true` iff `Status == LicenseStatus.Valid`.
* Failure codes MUST map deterministically to a `LicenseStatus`.

---

## Failure Model

```csharp
public sealed class LicenseValidationFailure
{
    public LicenseFailureCode Code { get; }

    public string Message { get; }

    public Exception? DiagnosticException { get; }
}
```

* `DiagnosticException` MUST be null unless diagnostics are explicitly enabled via configuration.

---

## Failure Codes (Initial Set)

```csharp
public enum LicenseFailureCode
{
    None = 0,

    InvalidJson,
    SchemaValidationFailed,

    UnsupportedEnvelopeVersion,
    UnsupportedProofSystem,
    UnsupportedStatement,

    StatementStructureInvalid,
    ProofVerificationFailed,

    LicenseExpired,

    InternalError
}
```

Mapping guidance:

* `Malformed` => `InvalidJson`, `SchemaValidationFailed`, `StatementStructureInvalid`
* `Unsupported` => `UnsupportedEnvelopeVersion`, `UnsupportedProofSystem`, `UnsupportedStatement`
* `Expired` => `LicenseExpired`
* `Invalid` => `ProofVerificationFailed`
* `Error` => `InternalError`

Validation MUST NOT throw for these failure codes.

---

## LicenseClaims (v1)

```csharp
public sealed class LicenseClaims
{
    public string ProductId { get; }

    public string Edition { get; }

    public IReadOnlyList<string> Features { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public int? MaxSeats { get; }

    public IReadOnlyDictionary<string, object> Extensions { get; }
}
```

* v1 statement MUST populate core properties.
* Future statements MAY use `Extensions`.

---

## Validation Pipeline (Normative Order)

1. Parse JSON.
2. Extract `envelopeVersion`, `proofSystem`, and `statementId`.
3. Validate `envelopeVersion` support.
4. Resolve `proofSystem` in registry.
5. Resolve `statementId` in registry.
6. Validate JSON Schema.
7. Validate structural compatibility between `statementId` and `publicInputs`.
8. Perform cryptographic proof verification.
9. Extract strongly-typed claims.
10. Apply deterministic semantic validation (e.g., expiry).
11. Return `LicenseValidationResult`.

Validation MUST fail fast at each stage and MUST fail closed.

---

## Registry Requirements

The SDK MUST maintain:

* A registry of supported `proofSystem` identifiers.
* A registry of supported `statementId` identifiers.

Registries MUST:

* Be configured via dependency injection.
* Be immutable at runtime (after application startup).
* Fail deterministically for unknown identifiers.

Example (illustrative):

```csharp
services.AddSigil(options =>
{
    options.RegisterProofSystem(new Groth16Verifier());
    options.RegisterStatement(new LicenseV1Statement());
});
```

---

## Time Semantics

Because expiry is evaluated by the SDK, the SDK MUST use an injectable clock abstraction to enable deterministic testing.

```csharp
public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
```

---

## JSON Schema Validation

The Proof Envelope JSON Schema for Spec 001 MUST be compiled/loaded once during service registration and reused for validation calls.

Schema validation MUST occur after identifier resolution and before proof verification.

---

## Diagnostics & Logging

* Validation failures MUST NOT expose sensitive cryptographic details by default.
* The SDK SHOULD use structured logging via `ILogger`.
* Detailed diagnostic information MAY be emitted at Debug level.
* Proof bytes MUST NOT be logged.
* Envelope JSON SHOULD NOT be logged in full.
* Diagnostic details in `LicenseValidationFailure` MUST only be populated when explicitly enabled via configuration.

---

## Exception Policy

* Validation failures MUST NOT throw.
* SDK misuse (e.g., null input) MAY throw `ArgumentException`.
* Unexpected internal faults MUST result in `Status = Error` with `InternalError`.

---

## Versioning Implications

* Adding support for new statements does NOT require envelope version change.
* Adding support for new proof systems does NOT require envelope version change.
* Breaking changes to the result model require a major SDK version bump.

---

## Acceptance Criteria

* SDK can validate minimal and full v1 envelopes.
* SDK returns deterministic `Status` and failure codes.
* Unsupported `statementId` fails with `Unsupported`.
* Unsupported `proofSystem` fails with `Unsupported`.
* Unsupported `envelopeVersion` fails with `Unsupported`.
* Expired licenses fail with `Expired`.
* No normal validation failure throws an exception.
* Schema is compiled once and reused.
