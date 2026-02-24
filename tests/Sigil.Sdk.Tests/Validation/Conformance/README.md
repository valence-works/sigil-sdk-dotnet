# Midnight Proof System Conformance Test Suite

**Feature**: Spec 006 — Midnight Proof System Verifier  
**Purpose**: Vector-driven conformance validation for Midnight ZK proof verification  
**Location**: `tests/Sigil.Sdk.Tests/Validation/Conformance/`

## Overview

The conformance test suite validates that the Midnight verifier (`MidnightZkV1ProofSystemVerifier`) produces deterministic, correct verification outcomes for known test vectors across valid proofs, cryptographically invalid proofs, and simulated internal errors.

## Requirements (from Spec 006)

- **FR-014**: Test harness MUST support a conformance-suite structure for Midnight verification vectors, including known-good and known-bad cases.
- **FR-015**: Conformance coverage MUST include, at minimum:
  - One known-valid vector for `urn:sigil:statement:license:v1`
  - One known-invalid vector
  - One simulated internal-error vector that verifies `Error` mapping

## Test Vector Structure

### Vector Format

Each test vector is a JSON file containing:

```json
{
  "testId": "midnight-valid-license-v1-001",
  "description": "Valid Midnight proof for license:v1 statement",
  "statementId": "urn:sigil:statement:license:v1",
  "proofSystem": "midnight-zk-v1",
  "proofBytes": "base64-encoded-proof-material",
  "expectedOutcome": "Verified",
  "expectedFailureCode": null,
  "context": {
    "statementData": {}
  },
  "source": "Community vector / Generated",
  "checksum": "sha256-hash-of-vector"
}
```

### File Organization

```
Vectors/
├── license-v1-valid.json           # Known-valid Midnight proof (Verified outcome)
├── license-v1-invalid.json         # Invalid proof bytes (Invalid outcome)
├── license-v1-internal-error.json  # Simulated internal error (Error outcome)
└── [future vectors for other statements]
```

## Test Execution

### Vector Loading

Vectors are loaded via `MidnightConformanceVectors` utility class:

```csharp
var vectors = MidnightConformanceVectors.LoadAll();
var validVector = vectors.FirstOrDefault(v => v.ExpectedOutcome == "Verified");
```

### Conformance Test Cases

1. **Valid Proof Test**: Submit known-valid vector, assert outcome is `Verified`, repeat 10+ times and verify determinism.
2. **Invalid Proof Test**: Submit known-invalid vector, assert outcome is `Invalid` with `ProofVerificationFailed`.
3. **Internal Error Test**: Submit error-simulation vector, assert outcome is `Error` with `ProofVerifierInternalError`.
4. **Determinism Test**: Run same valid vector 100+ times, verify 100% outcome consistency.

## Vector Acquisition

Vectors are acquired from:
- Midnight community test suites (if publicly available)
- Generated via documented Midnight proof generation process (if needed)
- See `docs/MIDNIGHT_TEST_VECTOR_ACQUISITION.md` for acquisition strategy

## Extending the Suite

To add new vectors (e.g., for additional statements):

1. Create new JSON file in `Vectors/` directory following the structure above
2. Update vector loader if new statement types are added
3. Add corresponding test cases in `MidnightProofConformanceTests.cs`
4. Document source and generation/acquisition process

## Success Criteria (from Spec 006 SC-006)

- ✅ Conformance suite includes minimum 3 required vectors (valid, invalid, error)
- ✅ All vectors are deterministic (identical inputs produce identical outputs on repeated runs)
- ✅ No `proofBytes` content is logged or emitted during vector testing
- ✅ All failure mappings align with Spec 002 status model

## References

- [Spec 006 Feature Specification](../../../../../../specs/006-midnight-proof-verifier/spec.md)
- [Spec 002 Validation API](../../../../../../specs/002-sdk-validation-api/spec.md)
- [Midnight Verifier Behavior Contract](../../../../../../specs/006-midnight-proof-verifier/contracts/midnight-verifier-behavior.md)
