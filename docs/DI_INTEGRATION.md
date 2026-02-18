# DI Integration Guide

## Overview

This guide provides comprehensive documentation for integrating Sigil SDK validation into .NET applications using dependency injection. The SDK provides a drop-in validator that requires minimal setup (3-5 lines) while offering extensive customization options for production scenarios.

**Quick Links**:
- [Minimal Setup](#minimal-setup)
- [Configuration Options](#configuration-options)
- [Extension Points](#extension-points)
- [Troubleshooting](#troubleshooting)
- [Production Best Practices](#production-best-practices)

---

## Minimal Setup

### ASP.NET Core Application

Add Sigil validation to your application with a single line in `Program.cs`:

```csharp
using Sigil.Sdk.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register Sigil validation services
builder.Services.AddSigilValidation();

// ... other service registrations

var app = builder.Build();
```

Inject the validator into controllers or services:

```csharp
using Sigil.Sdk.Validation;

public class ValidationController : ControllerBase
{
    private readonly ILicenseValidator validator;

    public ValidationController(ILicenseValidator validator)
    {
        this.validator = validator;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<LicenseValidationResult>> ValidateEnvelope(
        [FromBody] JsonElement envelope,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(envelope, cancellationToken);
        return Ok(result);
    }
}
```

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;
using Sigil.Sdk.Validation;

var services = new ServiceCollection();
services.AddSigilValidation();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ILicenseValidator>();

// Use validator
var result = await validator.ValidateAsync(envelopeJson);
```

### Worker Service

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sigil.Sdk.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSigilValidation();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
```

---

## Configuration Options

### Diagnostics Configuration

Control how validation failures are logged and diagnosed:

```csharp
builder.Services.AddSigilValidation(options =>
{
    // Enable diagnostic information collection
    options.EnableDiagnostics = true;
    
    // Control failure detail logging
    // Default: false (secure - logs only status and failure code)
    // Production: Enable only if logs are properly secured
    options.LogFailureDetails = true;
});
```

**Security Note**: `LogFailureDetails` defaults to `false` to prevent sensitive data leakage. Only enable in production environments where:
- Logs are properly secured and access-controlled
- Log output is never exposed to end users
- Application layer sanitizes sensitive data before logging

### Registered Services

`AddSigilValidation()` registers the following services as singletons:

| Service | Interface | Purpose |
|---------|-----------|---------|
| Validator | `ILicenseValidator` | Main validation entry point |
| Proof System Registry | `IProofSystemRegistry` | Immutable registry of proof verifiers |
| Statement Registry | `IStatementRegistry` | Immutable registry of statement handlers |
| Schema Validator | `IProofEnvelopeSchemaValidator` | Compiled JSON schema validator |
| Clock | `IClock` | Time abstraction (defaults to system clock) |
| Options | `ValidationOptions` | Configuration settings |

All services are thread-safe and suitable for concurrent use.

---

## Extension Points

### Registering Custom Proof Systems

Add custom proof system verifiers during service registration:

```csharp
using Sigil.Sdk.Proof;

public class CustomProofSystemVerifier : IProofSystemVerifier
{
    public async Task<bool> VerifyAsync(
        string statementId,
        JsonElement publicInputs,
        ReadOnlyMemory<byte> proofBytes,
        CancellationToken cancellationToken)
    {
        // Implement custom proof verification logic
        return true;
    }
}

// Register during setup
builder.Services.AddSigilValidation(options =>
{
    options.AddProofSystem("custom-zkp", new CustomProofSystemVerifier());
    options.AddProofSystem("groth16", new Groth16Verifier());
});
```

**Important Rules**:
- Proof system identifiers must be **non-empty, non-whitespace strings**
- **Duplicate identifiers throw `ArgumentException`** at registration time
- Registries are **immutable after container build** (Spec 003 FR-015)
- Custom verifiers are invoked during the validation pipeline

**Error Example**:
```csharp
// ❌ This will throw ArgumentException
options.AddProofSystem("groth16", verifier1);
options.AddProofSystem("groth16", verifier2); // Exception: duplicate identifier!

// ❌ This will throw ArgumentException
options.AddProofSystem("", verifier); // Exception: empty identifier!
options.AddProofSystem("   ", verifier); // Exception: whitespace-only identifier!
```

### Registering Custom Statement Handlers

Add custom statement handlers for application-specific validation:

```csharp
using Sigil.Sdk.Statements;

public class CustomStatementHandler : IStatementHandler
{
    public IReadOnlySet<string> HandledStatementIds => 
        new HashSet<string> { "custom-statement", "another-statement" };

    public async Task<ValidationResult> ValidateAsync(
        StatementData statement,
        CancellationToken cancellationToken)
    {
        // Implement custom statement validation logic
        return ValidationResult.Success();
    }
}

// Register during setup
builder.Services.AddSigilValidation(options =>
{
    options.AddStatementHandler(new CustomStatementHandler());
    options.AddStatementHandler(new LicenseExpiryHandler());
});
```

**Important Rules**:
- Each handler declares handled statement IDs via `HandledStatementIds` property
- **Duplicate statement IDs across handlers throw `ArgumentException`** at registration time
- Handlers are **immutable after container build** (Spec 003 FR-015)
- Handler instances are invoked during validation for matching statements

### Complete Extension Example

```csharp
builder.Services.AddSigilValidation(options =>
{
    // Enable diagnostics for production monitoring
    options.EnableDiagnostics = true;
    
    // Register custom proof systems
    options.AddProofSystem("paillier", new PaillierVerifier());
    options.AddProofSystem("groth16", new Groth16Verifier());
    
    // Register custom statement handlers
    options.AddStatementHandler(new LicenseExpiryHandler());
    options.AddStatementHandler(new FeatureLimitHandler());
    
    // Log failure details only in secure environments
    options.LogFailureDetails = IsProduction && LogsAreSecure;
});
```

---

## Troubleshooting

### Common Issues

#### "AddSigilValidation() called multiple times"

**Error**: `InvalidOperationException: AddSigilValidation() has already been called on this service collection.`

**Cause**: `AddSigilValidation()` was called more than once on the same `IServiceCollection`.

**Solution**: Call `AddSigilValidation()` exactly once during service registration.

#### "Duplicate proof system identifier"

**Error**: `ArgumentException: Proof system 'groth16' is already registered.`

**Cause**: `options.AddProofSystem()` was called multiple times with the same identifier.

**Solution**: Ensure each proof system has a unique identifier. Review registration code for duplicates.

#### "Duplicate statement handler"

**Error**: `ArgumentException: Statement ID 'license-expiry' is already handled by another handler.`

**Cause**: Multiple handlers declared the same statement ID in their `HandledStatementIds` property.

**Solution**: Ensure each statement ID is handled by at most one handler.

#### "Registries are immutable after container build"

**Error**: `InvalidOperationException: Registries are immutable after container build (Spec 003 FR-015). Use ValidationOptions during registration.`

**Cause**: Code attempted to modify proof system or statement registries after `BuildServiceProvider()` was called.

**Solution**: Register all custom systems and handlers during `AddSigilValidation()` configuration. Do not attempt runtime modification.

```csharp
// ❌ Wrong - attempting runtime modification
var registry = provider.GetRequiredService<IProofSystemRegistry>();
registry.AddVerifier("new-system", verifier); // Exception!

// ✓ Correct - register during setup
builder.Services.AddSigilValidation(options =>
{
    options.AddProofSystem("new-system", verifier);
});
```

#### "Missing required dependencies"

**Error**: `InvalidOperationException: Unable to resolve service for type 'ILicenseValidator'`

**Cause**: `AddSigilValidation()` was not called, or service provider was built incorrectly.

**Solution**: Ensure `AddSigilValidation()` is called before building the service provider.

---

## Production Best Practices

### Security

1. **Disable Detailed Logging by Default**
   ```csharp
   // Default is secure (LogFailureDetails = false)
   builder.Services.AddSigilValidation();
   ```

2. **Enable Detailed Logging Only in Secure Environments**
   ```csharp
   builder.Services.AddSigilValidation(options =>
   {
       // Only enable if logs are secured and sanitized
       options.LogFailureDetails = Configuration.GetValue<bool>("Logging:IncludeDetails");
   });
   ```

3. **Never Log Proof Bytes**
   - SDK automatically enforces this constraint (Spec 003 FR-016)
   - Application layer should sanitize any custom logging

### Performance

1. **Schema Compilation is Automatic**
   - Schema validator compiles once at startup (< 100ms)
   - No action required from developers

2. **Use Singleton Lifetime**
   - All SDK services are registered as singletons
   - Thread-safe for concurrent validation

3. **Memory Footprint**
   - Default configuration uses < 5 MB overhead (Spec 003 SC-004)
   - Suitable for embedded and containerized scenarios

### Observability

1. **Enable Diagnostics for Production Monitoring**
   ```csharp
   builder.Services.AddSigilValidation(options =>
   {
       options.EnableDiagnostics = true;
   });
   ```

2. **Integrate with Application Logging**
   - SDK uses `ILogger<LicenseValidator>` for structured logging
   - Configure logging in `appsettings.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Sigil.Sdk": "Information"
       }
     }
   }
   ```

### Testing

1. **Mock the Validator in Unit Tests**
   ```csharp
   var mockValidator = new Mock<ILicenseValidator>();
   mockValidator
       .Setup(v => v.ValidateAsync(It.IsAny<JsonElement>(), default))
       .ReturnsAsync(LicenseValidationResult.Valid());
   
   var controller = new ValidationController(mockValidator.Object);
   ```

2. **Use Test Containers for Integration Tests**
   ```csharp
   var services = new ServiceCollection();
   services.AddSigilValidation();
   var provider = services.BuildServiceProvider();
   
   var validator = provider.GetRequiredService<ILicenseValidator>();
   var result = await validator.ValidateAsync(testEnvelope);
   Assert.Equal(LicenseStatus.Valid, result.Status);
   ```

---

## Advanced Topics

### Custom Clock Implementation

Replace the default system clock for testing or custom time logic:

```csharp
public class CustomClock : IClock
{
    public DateTimeOffset UtcNow => /* custom logic */;
}

// Note: Custom clock registration requires manual service replacement
// The SDK does not provide a built-in option for this
```

### Registry Inspection

Check which proof systems and statement handlers are registered:

```csharp
var proofRegistry = provider.GetRequiredService<IProofSystemRegistry>();
if (proofRegistry.TryGetVerifier("groth16", out var verifier))
{
    // Verifier is registered
}

var statementRegistry = provider.GetRequiredService<IStatementRegistry>();
if (statementRegistry.TryGetHandler("license-expiry", out var handler))
{
    // Handler is registered
}
```

---

## Sample Projects

See [samples/MinimalDiSample](../samples/MinimalDiSample) for a complete working example that demonstrates:
- Minimal 3-line setup
- ASP.NET Core integration
- REST API endpoint
- Swagger/OpenAPI documentation
- Custom diagnostics configuration
- Extension point usage

---

## References

- [Specification: DI Integration & Defaults](../specs/003-di-integration/spec.md)
- [Architecture Documentation](./architecture.md)
- [Public API Reference](../specs/002-sdk-validation-api/contracts/public-api.md)
- [Minimal Sample](../samples/MinimalDiSample/README.md)

---

**Spec 003 (T051)**: Comprehensive DI integration guide for .NET developers  
**Last Updated**: 2026-02-17
