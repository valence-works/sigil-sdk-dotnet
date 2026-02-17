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

### Add Custom Proof Systems

In Program.cs, customize the setup:

```csharp
builder.Services.AddSigilValidation(options =>
{
    options.AddProofSystem("paillier", new PaillierVerifier());
    options.AddProofSystem("groth16", new Groth16Verifier());
});
```

### Add Custom Statement Handlers

```csharp
builder.Services.AddSigilValidation(options =>
{
    options.AddStatementHandler(new MyStatementHandler());
});
```

### Enable Diagnostics Logging

```csharp
builder.Services.AddSigilValidation(options =>
{
    options.EnableDiagnostics = true;
    options.LogFailureDetails = true; // Include detailed failure info
});
```

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
