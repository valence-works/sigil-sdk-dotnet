# Contract — `license:v1` Statement Definition (Spec 004)

## Statement Identifier

- `statementId`: `urn:sigil:statement:license:v1`

## `publicInputs` Shape

`publicInputs` MUST be a JSON object and MUST include only documented fields.

### Required Fields

- `productId`: string, non-empty
- `edition`: string, non-empty
- `features`: array of string, unique entries, lowercase kebab-case
- `expiresAt`: integer (unix epoch seconds)
- `maxSeats`: integer (`> 0`)

### Optional Fields

- `issuedAt`: integer (unix epoch seconds)
- `metadata`: object

### Strictness

- Unknown fields MUST be rejected deterministically.

## Validation Rules

1. Missing required field -> invalid.
2. Required field null -> invalid.
3. Type mismatch -> invalid.
4. `features` containing duplicates or non-kebab-case entries -> invalid.
5. `maxSeats <= 0` -> invalid.

## Claim Extraction Mapping

On success, handler MUST extract:

- `ProductId` <- `productId`
- `Edition` <- `edition`
- `Features` <- `features`
- `ExpiresAt` <- `expiresAt`
- `MaxSeats` <- `maxSeats`
- `IssuedAt` <- `issuedAt` (only when present)

## Output Contract

- Valid input -> `IsValid=true`, non-null claims with mapped values.
- Invalid input -> `IsValid=false`, null claims.
