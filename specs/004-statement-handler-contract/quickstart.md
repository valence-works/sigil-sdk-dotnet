# Quickstart — Statement Handler Contract & `license:v1` (Spec 004)

This quickstart shows expected usage and verification flow for statement handlers.

## Build Environment

- Build/tooling SDK: .NET 10
- Library target: `net8.0`

## Implement a custom statement handler

```csharp
public sealed class CustomStatementHandler : IStatementHandler
{
    public Task<StatementValidationResult> ValidateAsync(
        JsonElement publicInputs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (publicInputs.ValueKind != JsonValueKind.Object)
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        var claims = new LicenseClaims(
            productId: "demo-product",
            edition: "pro",
            features: new[] { "offline-validation" },
            expiresAt: DateTimeOffset.UtcNow.AddDays(30),
            maxSeats: 10,
            issuedAt: DateTimeOffset.UtcNow);

        return Task.FromResult(new StatementValidationResult(true, claims));
    }
}
```

## Register handler with DI

```csharp
services.AddSigilValidation(options =>
{
    options.AddStatementHandler(new LicenseV1StatementHandler());
    options.AddStatementHandler(new CustomStatementHandler());
});
```

## Expected outcomes

- Invalid statement inputs return result-object failures (no validation exceptions).
- Unknown `license:v1` fields are rejected deterministically.
- `issuedAt` is included in claims when present.
- Expiry is evaluated by validator after statement validation.
