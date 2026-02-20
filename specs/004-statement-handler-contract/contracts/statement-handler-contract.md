# Contract — Statement Handler API (Spec 004)

## Scope

Defines normative behavior for `IStatementHandler` implementations used by the validator.

## Interface Contract

```csharp
string StatementId { get; }

Task<StatementValidationResult> ValidateAsync(
    JsonElement publicInputs,
    CancellationToken cancellationToken = default);
```

## Normative Requirements

1. Handler MUST validate `publicInputs` according to statement-specific rules.
2. Handler MUST be deterministic for identical inputs.
3. Handler MUST respect cooperative cancellation:
   - If cancellation is signaled before completion, handler MAY throw `OperationCanceledException`.
4. Handler MUST NOT throw for validation failures; validation failures are returned as result data.
5. Handler MUST return:
   - `IsValid=false, Claims=null` on semantic/shape/type validation failure.
   - `IsValid=true, Claims!=null` on success.
6. Handler MUST NOT mutate input `publicInputs`.
7. Handler MUST NOT emit full `publicInputs` payloads or `proofBytes` into logs.

## Invalid Contract States

- `IsValid=true` with `Claims=null` is invalid contract output and MUST be treated as validation failure by the validator.

## Registry Contract

- Handler instances are resolved from immutable DI-registered statement registry.
- Registry keys MUST be derived from `StatementId`.
- Duplicate statement registration is a startup misconfiguration error.
