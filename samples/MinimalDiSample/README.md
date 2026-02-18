# Minimal DI Sample - Sigil Validator Integration

This sample demonstrates the minimal setup required to integrate Sigil validation into an ASP.NET Core application.

## Features

- **Minimal Setup (3-5 lines)**: Single `AddSigilValidation()` call in Program.cs
- **Dependency Injection**: ISigilValidator is automatically resolved
- **REST API**: Example controller showing how to use the validator
- **Swagger/OpenAPI**: Interactive API documentation for testing
- **Default Configuration**: Secure defaults with no additional setup required

## Quick Start

### 1. Build and Run

```bash
cd samples/MinimalDiSample
dotnet run
```

The application will start on `https://localhost:44300` (or `http://localhost:5000`).

### 2. Test with Swagger UI

Navigate to `https://localhost:44300/swagger` to access the interactive API documentation.

### 3. Test Validation Endpoint

Use the `/api/validation/validate` POST endpoint with a proof envelope JSON string in the request body:

```
POST https://localhost:44300/api/validation/validate
Content-Type: application/json

"{\"version\":\"1.0\",\"proof\":{\"proofSystem\":\"unknown\",\"publicInputs\":{},\"proofBytes\":\"AQIDBA==\"},\"statements\":[]}"
```

Or using curl:

```bash
curl -X POST https://localhost:44300/api/validation/validate \
  -H "Content-Type: application/json" \
  -d '"{\"version\":\"1.0\",\"proof\":{\"proofSystem\":\"unknown\",\"publicInputs\":{},\"proofBytes\":\"AQIDBA==\"},\"statements\":[]}"'
```

## Code Overview

### Program.cs (3-5 Line Setup)

```csharp
// Spec 003 (FR-001): Single line registration with defaults
builder.Services.AddSigilValidation();

// Optional: Add custom configuration
// builder.Services.AddSigilValidation(options =>
// {
//     options.EnableDiagnostics = true;
// });
```

This single call registers:
- `ISigilValidator` - Main validation service
- `IProofSystemRegistry` - Empty by default (add custom systems via options)
- `IStatementRegistry` - Empty by default (add handlers via options)
- `IProofEnvelopeSchemaValidator` - Compiled schema validator
- `IClock` - System clock for time-based operations
- `ValidationOptions` - Configuration with secure defaults

### ValidationController.cs (Usage Example)

```csharp
public class ValidationController : ControllerBase
{
    private readonly ILicenseValidator validator;

    // Validator is injected by the DI container
    public ValidationController(ILicenseValidator validator)
    {
        this.validator = validator;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<LicenseValidationResult>> ValidateProofEnvelope(
        [FromBody] JsonElement envelope,
        CancellationToken cancellationToken)
    {
        // Use validator directly - all dependencies are already wired
        var result = await validator.ValidateAsync(envelope, cancellationToken);
        return Ok(result);
    }
}
```

## Customization

### Extending with Custom Proof Systems

Register custom proof system verifiers during the initial `AddSigilValidation()` call:

```csharp
builder.Services.AddSigilValidation(options =>
{
    // Register custom proof systems by identifier
    options.AddProofSystem("paillier", new PaillierVerifier());
    options.AddProofSystem("groth16", new Groth16Verifier());
    options.AddProofSystem("custom-zkp", new CustomZkpVerifier());
});
```

**Important Notes**:
- Proof system identifiers must be **non-empty, non-whitespace strings**
- **Duplicate identifiers are not allowed** - calling `AddProofSystem()` with the same identifier twice throws `ArgumentException`
- Verifiers are registered in an **immutable registry** after container build (Spec 003 FR-015)
- Custom verifiers must implement `IProofSystemVerifier` interface
- Verifier instances are invoked during the validation pipeline

**Error Handling**:
```csharp
// ❌ This will throw ArgumentException (duplicate identifier)
options.AddProofSystem("groth16", verifier1);
options.AddProofSystem("groth16", verifier2); // Exception!

// ❌ This will throw ArgumentException (empty identifier)
options.AddProofSystem("", verifier); // Exception!
options.AddProofSystem("   ", verifier); // Exception!
```

### Extending with Custom Statement Handlers

Register custom statement handlers to validate specific statement types:

```csharp
builder.Services.AddSigilValidation(options =>
{
    // Register custom handlers by statement type
    options.AddStatementHandler(new LicenseExpiryHandler());
    options.AddStatementHandler(new FeatureLimitHandler());
    options.AddStatementHandler(new CustomBusinessRuleHandler());
});
```

**Important Notes**:
- Statement handlers must implement `IStatementHandler` interface
- Each handler declares which statement types it handles via `HandledStatementIds` property
- **Duplicate statement IDs are not allowed** - registering multiple handlers for the same ID throws `ArgumentException`
- Handlers are registered in an **immutable registry** after container build (Spec 003 FR-015)
- Handler instances are invoked during the validation pipeline

**Error Handling**:
```csharp
// ❌ This will throw ArgumentException if both handle same statement ID
options.AddStatementHandler(handler1); // handles "license-expiry"
options.AddStatementHandler(handler2); // also handles "license-expiry" - Exception!
```

### Extension Point Pattern Example

Complete example showing proof system and statement handler extension:

```csharp
public class CustomProofSystemVerifier : IProofSystemVerifier
{
    public string ProofSystemId => "custom-zkp";

    public Task<bool> VerifyAsync(
        IReadOnlyDictionary<string, object> publicInputs,
        ReadOnlyMemory<byte> proofBytes,
        CancellationToken cancellationToken)
    {
        // Implement custom proof verification logic
        return Task.FromResult(true);
    }
}

public class CustomStatementHandler : IStatementHandler
{
    public IReadOnlySet<string> HandledStatementIds => 
        new HashSet<string> { "custom-statement" };

    public Task<ValidationResult> ValidateAsync(
        StatementData statement,
        CancellationToken cancellationToken)
    {
        // Implement custom statement validation logic
        return Task.FromResult(ValidationResult.Success());
    }
}

// Register both in Program.cs
builder.Services.AddSigilValidation(options =>
{
    options.AddProofSystem("custom-zkp", new CustomProofSystemVerifier());
    options.AddStatementHandler(new CustomStatementHandler());
});
```

### Enable Diagnostics Logging

Configure diagnostic logging for production environments (Spec 003 T023-T027):

```csharp
builder.Services.AddSigilValidation(options =>
{
    // Enable collection of diagnostic information
    options.EnableDiagnostics = true;
    
    // Include detailed failure messages in application logs
    // Default: false (secure default - logs only status and failure code)
    // Set to true only in production if logs are properly secured
    options.LogFailureDetails = true;
});
```

**Security Note**: By default, `LogFailureDetails` is `false`. This ensures sensitive failure details are never logged unless explicitly enabled. Only enable detailed logging in production environments where logs are:
- Properly secured and access-controlled
- Not exposed in error responses to clients
- Sanitized of any sensitive data by the application layer

**Best Practice**: In development, enable LogFailureDetails for detailed diagnostics. In production, only enable if logs are securely managed.

## Architecture

The sample demonstrates:

- **Dependency Injection**: All validator dependencies injected automatically
- **Separation of Concerns**: Controller focuses on HTTP logic; validator handles business logic
- **Configuration**: Builder pattern for clean, fluent API
- **Testing**: All services are mockable via DI

## See Also

- [Sigil SDK Documentation](../../docs/)
- [DI Integration Guide](../../docs/DI_INTEGRATION.md)
- [Specification (Spec 003)](../../specs/003-di-integration/spec.md)

## Notes

- Default configuration is production-ready with secure defaults (diagnostics off, no sensitive data logging)
- Schema validator is compiled once at startup for optimal performance
- Registries are immutable after container is built
- This sample uses .NET 8.0; compatible with .NET 10

Spec 003 (SC-001): <5 minutes integration time with ≤5 lines of code ✓
