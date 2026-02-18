# Research: DI Integration & Defaults

## Decision: DI extension method behavior on multiple calls

- Decision: Throw `InvalidOperationException` on a second call to `AddSigilValidation`.
- Rationale: Prevents accidental double-registration and keeps configuration deterministic.
- Alternatives considered: Idempotent `TryAdd` registrations; last-call-wins overrides.

## Decision: Duplicate proof system or statement handler identifiers

- Decision: Throw `InvalidOperationException` immediately when a duplicate identifier is registered.
- Rationale: Duplicate identifiers introduce nondeterministic routing; failing fast is clearer and safer.
- Alternatives considered: First-wins, last-wins, or warnings with overrides.

## Decision: Options and logging integration approach

- Decision: Provide an optional configuration delegate for `ValidationOptions` and rely on host-configured logging (`ILogger<T>`).
- Rationale: Keeps setup minimal while integrating with standard .NET host logging without SDK-owned providers.
- Alternatives considered: SDK-configured logging providers; requiring `IOptions<ValidationOptions>` as the only path.

## Decision: Service lifetimes

- Decision: Use singletons for schema validator, registries, clock, and validator services.
- Rationale: Aligns with immutable registries, compiled schema reuse, and deterministic validation.
- Alternatives considered: Scoped lifetimes for validator services; transient schema validator creation.