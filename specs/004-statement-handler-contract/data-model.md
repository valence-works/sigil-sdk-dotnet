# Data Model — Statement Handler Contract & license:v1 (Spec 004)

**Spec**: [specs/004-statement-handler-contract/spec.md](spec.md)  
**Depends on**: Spec 001 Proof Envelope Format, Spec 002 SDK Validation API, Spec 003 DI Integration

## Primary Entities

### Statement Definition

Represents semantic validation rules and claim extraction mapping keyed by `statementId`.

Core fields:
- `statementId` (URN, required)
- `publicInputsRules` (required shape/type/value constraints)
- `claimMapping` (required mapping from validated inputs to `LicenseClaims`)
- `expirySemantics` (whether and how `expiresAt` contributes to expiration outcomes)

### Statement Handler (`IStatementHandler`)

Contract implementation for one statement definition.

Inputs:
- `publicInputs` (`JsonElement`)
- `CancellationToken`

Outputs:
- `StatementValidationResult` with:
  - `IsValid` (`bool`)
  - `Claims` (`LicenseClaims?`) with invariant: if `IsValid == true`, claims are non-null.

Behavioral invariants:
- Deterministic for same input.
- No mutation of input payload.
- Cooperative cancellation support.

### LicenseV1PublicInputs

Canonical shape for `urn:sigil:statement:license:v1`.

Required fields:
- `productId` (`string`, non-empty)
- `edition` (`string`, non-empty)
- `features` (`string[]`, lowercase kebab-case, unique)
- `expiresAt` (`long`, unix seconds)
- `maxSeats` (`int`, `> 0`)

Optional fields:
- `issuedAt` (`long`, unix seconds)
- `metadata` (`object`)

Strictness rule:
- Unknown fields are rejected.

### LicenseClaims

Validated claim output used by consumers and pipeline result synthesis.

Mapped fields:
- `ProductId`
- `Edition`
- `Features`
- `ExpiresAt`
- `MaxSeats`
- `IssuedAt` (optional; present only when source value exists)

### Expiry Evaluation Outcome

Pipeline-level semantic outcome derived from validated `expiresAt`.

States:
- Not expired (continues as valid path)
- Expired (maps deterministically to `LicenseStatus.Expired` with corresponding failure code)

## Relationships

- One `StatementDefinition` is implemented by exactly one handler instance in registry resolution.
- `license:v1` handler validates `LicenseV1PublicInputs` and produces `LicenseClaims`.
- Expiry evaluation consumes validated `LicenseClaims.ExpiresAt`.

## Validation State Transitions

1. `publicInputs` received by statement handler.
2. Shape/type/value checks applied.
3. On validation failure: return invalid result with null claims.
4. On validation success: map claims, enforce non-null claims invariant.
5. Validator executes expiry stage using validated claim values.
