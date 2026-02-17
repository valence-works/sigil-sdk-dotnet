# Quickstart — SDK Validation API (Spec 002)

This quickstart shows how an application uses Sigil SDK to validate a Spec 001 Proof Envelope.

## Validate from JSON string

```csharp
var result = await validator.ValidateAsync(envelopeJson, cancellationToken);

if (result.Status == LicenseStatus.Valid)
{
    // Allow: use typed claims.
    var claims = result.Claims;
}
else
{
    // Deny: branch on coarse status + deterministic failure code.
    var status = result.Status;
    var code = result.Failure?.Code;
}
```

## Validate from Stream

```csharp
await using var stream = File.OpenRead(path);
var result = await validator.ValidateAsync(stream, cancellationToken);
```

## Operational Notes

- Validation is offline and fail-closed: `Valid` is returned only when all steps succeed.
- Schema validation runs before cryptographic verification.
- `proofBytes` are never logged; diagnostics are opt-in.
