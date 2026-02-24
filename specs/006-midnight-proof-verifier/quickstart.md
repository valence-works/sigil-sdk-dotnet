# Quickstart: Midnight Proof System Verifier

**Feature**: Spec 006 — Midnight Proof System Verifier  
**Audience**: SDK consumers integrating Midnight verification  
**Date**: 2026-02-24

## Overview

The Midnight Proof System Verifier (`midnight-zk-v1`) is the first concrete proof system implementation for the Sigil SDK. It provides deterministic, offline cryptographic verification for Midnight zero-knowledge proofs within the SDK validation pipeline.

## Prerequisites

- .NET 8.0 or later runtime
- Sigil SDK package (includes Specs 001-006)
- Basic familiarity with DI setup (Spec 003) and validation API (Spec 002)

## Quick Setup (Default Configuration)

The simplest integration uses the default DI configuration, which automatically registers the Midnight verifier:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;

var services = new ServiceCollection();

// Register Sigil validator with Midnight verifier included by default
services.AddSigilValidation();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ILicenseValidator>();
```

**What this does**:
- Registers the Midnight verifier with identifier `midnight-zk-v1`
- Wires the verifier into the immutable proof system registry
- Makes the verifier available for envelopes declaring `proofSystem: "midnight-zk-v1"`

## Validating a Midnight Proof Envelope

Once the validator is registered, simply call `ValidateAsync` with a Midnight envelope:

```csharp
string envelopeJson = @"{
  ""envelopeVersion"": ""1.0"",
  ""proofSystem"": ""midnight-zk-v1"",
  ""statementId"": ""urn:sigil:statement:license:v1"",
  ""proofBytes"": ""..."",
  ""publicInputs"": {
    ""subject"": ""ProductX"",
    ""edition"": ""Enterprise"",
    ""features"": [""feature-a"", ""feature-b""],
    ""expiresAt"": 1735689600,
    ""maxSeats"": 100
  }
}";

var result = await validator.ValidateAsync(envelopeJson);

if (result.Status == LicenseStatus.Valid)
{
    Console.WriteLine($"License valid for {result.Claims?.Product}");
}
else
{
    Console.WriteLine($"Validation failed: {result.Status} - {result.Failure?.Code}");
}
```

## Understanding Validation Outcomes

The Midnight verifier produces deterministic outcomes that map to standard `LicenseStatus` values:

| Verifier Outcome | Maps To Status | Typical Cause |
|------------------|----------------|---------------|
| Verified | `Valid` (if expiry passes) | Cryptographic verification succeeded |
| Invalid | `Invalid` | Proof verification failed or statement context incompatible |
| Error | `Error` | Internal verifier fault (unexpected exception) |

### Example: Cryptographic Verification Failure

```csharp
// Envelope with invalid proof
var result = await validator.ValidateAsync(invalidEnvelopeJson);

Assert.Equal(LicenseStatus.Invalid, result.Status);
Assert.Equal(LicenseFailureCode.ProofVerificationFailed, result.Failure.Code);
```

### Example: Statement Context Incompatibility

```csharp
// Midnight verifier only supports license:v1 initially
string unsupportedStatement = @"{
  ""proofSystem"": ""midnight-zk-v1"",
  ""statementId"": ""urn:sigil:statement:custom:v1"",
  ...
}";

var result = await validator.ValidateAsync(unsupportedStatement);

Assert.Equal(LicenseStatus.Invalid, result.Status);
Assert.Equal(LicenseFailureCode.ProofVerificationContextIncompatible, result.Failure.Code);
```

## Explicit Configuration (Advanced)

If you need explicit control over the Midnight verifier registration (e.g., for testing alternate configurations), use the configuration delegate:

```csharp
services.AddSigilValidation(options =>
{
    // Midnight verifier is added by default, but you can control other settings
    options.EnableDiagnostics = true;  // Opt-in diagnostics (never logs proofBytes)
    options.LogFailureDetails = false; // Secure default (no sensitive data in logs)
});
```

**Note**: The Midnight verifier is registered by default in `AddSigilValidation()`. To exclude it or use a custom implementation, you would need to manually configure `ValidationOptions` without calling `AddMidnightZkV1ProofSystem()`.

## Performance Expectations

The Midnight verifier is designed to stay within the SDK performance budget:

- **End-to-end validation target**: p95 < 1 second for envelopes ≤ 10KB (Spec 002)
- **Midnight budget share**: ≤ 60% of that p95 (≤ 600ms)

**Best practices**:
- Reuse the validator singleton across requests (DI handles this automatically)
- Avoid creating new `ServiceProvider` instances per validation
- Use the SDK in warm processes (the first validation may include initialization overhead)

## Error Handling

### Standard Failures (Expected)

Validation failures (invalid proofs, expired licenses) are returned as result objects, not exceptions:

```csharp
var result = await validator.ValidateAsync(envelopeJson);

// No exception thrown; check result.Status
if (result.Status != LicenseStatus.Valid)
{
    // Handle failure gracefully
    LogFailure(result.Failure.Code);
}
```

### Exceptions (Programmer Errors)

The SDK only throws exceptions for programmer errors:

```csharp
try
{
    await validator.ValidateAsync(null); // Programmer error
}
catch (ArgumentNullException ex)
{
    // Fix the calling code
}
```

**Internal verifier faults** (e.g., corrupted state, unexpected exceptions) are mapped to `LicenseStatus.Error` and returned as result objects (not thrown).

## Security & Confidentiality

The Midnight verifier strictly enforces proof confidentiality:

- `proofBytes` are **never logged** at any log level
- Diagnostics (if enabled) **redact proof material**
- Error messages **never expose** `proofBytes` or sensitive verifier internals

**Safe to enable**:
```csharp
options.EnableDiagnostics = true;  // Safe: diagnostics redact proof material
```

**Example diagnostic output** (when enabled):
```text
[ValidationDiagnostic] proofSystem=midnight-zk-v1, statementId=urn:sigil:statement:license:v1, failureCode=ProofVerificationFailed
```

(Note: no `proofBytes` content visible)

## Testing with Conformance Vectors

The SDK includes conformance vectors for testing Midnight verification:

```csharp
// tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/
// - license-v1-valid.json
// - license-v1-invalid.json
// - license-v1-internal-error.json

[Fact]
public async Task KnownValidVector_ReturnsVerified()
{
    var vectors = MidnightConformanceVectors.Load();
    var validVector = vectors.Single(v => v.VectorId == "license-v1-valid");

    var result = await validator.ValidateAsync(validVector.EnvelopeJson);

    Assert.Equal(LicenseStatus.Valid, result.Status);
}
```

## Troubleshooting

### Issue: Validation returns `Unsupported` for `midnight-zk-v1`

**Cause**: Midnight verifier not registered in DI.

**Fix**: Ensure `AddSigilValidation()` is called (Midnight is included by default). If manually configuring, call `options.AddMidnightZkV1ProofSystem()`.

### Issue: Cryptographic verification always returns `Invalid`

**Possible causes**:
1. Proof is actually invalid (expected behavior)
2. Statement context mismatch (check that envelope uses `statementId: "urn:sigil:statement:license:v1"`)
3. Proof system version mismatch (verify envelope uses `proofSystem: "midnight-zk-v1"` exactly)

**Debug steps**:
- Enable diagnostics: `options.EnableDiagnostics = true`
- Check failure code: `result.Failure.Code` (distinguishes crypto failure from context issues)
- Use conformance vectors to verify baseline SDK behavior

### Issue: Validation slower than expected

**Check**:
- Are you creating new `ServiceProvider` instances per validation? (Reuse the singleton validator)
- Is the process warm? (First validation may include initialization)
- Are envelopes > 10KB? (Performance target is for envelopes ≤ 10KB)

## Next Steps

- **Integrate into your app**: Add `AddSigilValidation()` to your startup DI registration
- **Test with your proofs**: Validate real Midnight envelopes from your issuance system
- **Review Spec 002**: Understand the full validation pipeline (schema → crypto → expiry)
- **Review Spec 004**: Understand statement handler semantics for `license:v1`

## Reference

- **Spec 006**: [spec.md](spec.md) — Full Midnight verifier requirements
- **Spec 005**: [../005-proof-system-verifier-contract/spec.md](../005-proof-system-verifier-contract/spec.md) — Proof system verifier contract
- **Spec 002**: [../002-sdk-validation-api/spec.md](../002-sdk-validation-api/spec.md) — Validation API and pipeline
- **Contract**: [contracts/midnight-verifier-behavior.md](contracts/midnight-verifier-behavior.md) — Detailed behavioral contract
