# Data Model: Proof Envelope Format

## Entities

### ProofEnvelope

**Description**: Versioned JSON document that packages proof bytes and public inputs for offline verification.

**Fields**:
- `envelopeVersion` (string, required): Must be "1.0" for v1.
- `proofSystem` (string, required): Identifier of verification mechanism.
- `statementId` (string, required): URN identifying semantic contract.
- `proofBytes` (string, required): Base64-encoded proof bytes.
- `publicInputs` (object, required): Fixed schema v1 claims.
- `issuedAt` (string, optional): ISO 8601 timestamp.
- `policyHash` (string, optional): Identifier/hash for issuance policy snapshot.
- `extensions` (object, optional): Forward-compatible extension object.

**Validation rules**:
- Unknown top-level fields are ignored for forward compatibility.
- Unknown `proofSystem` or `statementId` must fail deterministically.
- Fail closed on any schema or integrity violation.
- Do not log or emit `proofBytes`.

### PublicInputs

**Description**: Fixed schema claims that bind the proof to a product and license parameters.

**Fields**:
- `productId` (string, required)
- `edition` (string, required)
- `features` (array of string, required): Lowercase kebab-case, unique.
- `expiresAt` (string, optional): ISO 8601 timestamp. Expired inputs fail.
- `maxSeats` (integer, optional): Must be >= 1 if present.

**Validation rules**:
- Strict schema: unknown fields fail validation.
- `expiresAt` is compared to current UTC time.

## Relationships

- ProofEnvelope contains exactly one PublicInputs object.
