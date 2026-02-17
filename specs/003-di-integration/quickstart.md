# Quickstart: DI Integration & Defaults

## Minimal Setup (3-5 Lines)

```csharp
var services = new ServiceCollection();
services.AddSigilValidation();
var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ISigilValidator>();
```

## Optional Diagnostics

```csharp
services.AddSigilValidation(options => options.EnableDiagnostics = true);
```

## Extension Points

```csharp
services.AddSigilValidation(options =>
{
    options.AddProofSystem("custom", customVerifier);
    options.AddStatementHandler(customStatementHandler);
});
```

## Sample

See the sample integration in `/samples` for a complete working example.