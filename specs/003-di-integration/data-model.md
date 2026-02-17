# Data Model: DI Integration & Defaults

## Entities

### ValidationOptions

- **Purpose**: Configuration for diagnostics and logging behavior during validation.
- **Fields**:
  - `EnableDiagnostics` (bool): Enables opt-in diagnostics output.
- **Validation Rules**:
  - Defaults to `false` (no diagnostics) when not configured.

### ProofSystemRegistration

- **Purpose**: Associates a `proofSystem` identifier with a verifier implementation.
- **Fields**:
  - `Identifier` (string): Unique proof system identifier.
  - `Verifier` (IProofSystemVerifier): Verifier implementation.
- **Validation Rules**:
  - `Identifier` must be unique; duplicates fail fast with `InvalidOperationException`.

### StatementHandlerRegistration

- **Purpose**: Associates a `statementId` with a statement handler implementation.
- **Fields**:
  - `Identifier` (string): Unique statement identifier.
  - `Handler` (IStatementHandler): Handler implementation.
- **Validation Rules**:
  - `Identifier` must be unique; duplicates fail fast with `InvalidOperationException`.

### Immutable Registries

- **Purpose**: Provide deterministic resolution of proof systems and statements.
- **Fields**:
  - `ProofSystemRegistry`: Map of proof system identifiers to verifiers.
  - `StatementRegistry`: Map of statement identifiers to handlers.
- **Validation Rules**:
  - Registry contents are immutable after DI container build.

### SchemaValidator

- **Purpose**: Performs Spec 001 schema validation using a compiled schema.
- **Fields**:
  - `CompiledSchema`: Pre-compiled JSON schema artifact.
- **Validation Rules**:
  - Compiled once at startup and reused.

### Clock

- **Purpose**: Provides the current time for validation operations.
- **Fields**:
  - `UtcNow`: Current UTC timestamp.
- **Validation Rules**:
  - Default implementation is system clock.

## Relationships

- `AddSigilValidation` constructs immutable registries from registrations and injects them into the validation pipeline.
- `ISigilValidator` depends on the schema validator, registries, options, and clock.
- `ValidationOptions` influences logging/diagnostics behavior in validation and logging helpers.

## State Transitions

- Registrations are mutable during configuration.
- Registries become immutable after service container build and cannot be modified at runtime.