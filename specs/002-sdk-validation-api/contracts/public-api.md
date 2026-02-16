# Contract — Public API Surface (Spec 002)

This document defines the externally-visible SDK API contract for validating Spec 001 Proof Envelopes.

## Entry Points

The SDK MUST provide these asynchronous methods:

```csharp
Task<LicenseValidationResult> ValidateAsync(
    string envelopeJson,
    CancellationToken cancellationToken = default);

Task<LicenseValidationResult> ValidateAsync(
    Stream envelopeStream,
    CancellationToken cancellationToken = default);
```

Notes:
- Validation failures MUST be represented in the returned `LicenseValidationResult` (no exceptions for expected validation failures).
- Exceptions may be thrown only for programmer errors (e.g., null arguments) or gross misconfiguration, but the primary contract for invalid inputs is a result object.

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

public sealed class LicenseValidationFailure
{
    public LicenseFailureCode Code { get; }

    public string Message { get; }

    public Exception? DiagnosticException { get; }
}
```

Contract rules:
- `IsValid` MUST be `true` iff `Status == LicenseStatus.Valid`.
- `DiagnosticException` MUST be null unless diagnostics are explicitly enabled.
- `Message` MUST be safe (must not include `proofBytes` or raw envelope JSON).
