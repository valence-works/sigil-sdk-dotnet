# Public API Contract: DI Integration & Defaults

## Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSigilValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configure = null);
}
```

## Configuration Extension Points

```csharp
public sealed class ValidationOptions
{
    public bool EnableDiagnostics { get; init; }

    public void AddProofSystem(string identifier, IProofSystemVerifier verifier);
    public void AddStatementHandler(IStatementHandler handler);
}
```

## Error Behavior

- Calling `AddSigilValidation` more than once on the same `IServiceCollection` throws `InvalidOperationException`.
- Registering duplicate proof system identifiers throws `InvalidOperationException`.
- Registering duplicate statement handler identifiers throws `InvalidOperationException`.
- Modifying registries after container build throws `InvalidOperationException`.
- Missing or misconfigured required dependencies throw `InvalidOperationException` during registration or container build.