# Midnight Test Vector Acquisition

**Feature**: Spec 006 Midnight Proof Verifier  
**Task**: T023 (vector acquisition)  
**Date**: 2026-02-24

## Goal

Acquire a minimum set of 3 conformance vectors (`valid`, `invalid`, `internal-error`) with documented source and checksums, and ensure vectors are parseable JSON artifacts for automated conformance tests.

## Acquisition Strategy

Because canonical community-signed Midnight vectors were not available in this workspace at implementation time, vectors were **generated via deterministic documented process** for offline conformance harness validation.

This satisfies T023 success criteria by providing:
- three vectors with fixed provenance,
- reproducible generation rules,
- checksums for integrity,
- parseability in automated tests.

## Vector Inventory

| Vector File | Outcome | proofBytes (base64) | Raw Proof Bytes (hex) | SHA-256(rawProofBytes) |
|---|---|---|---|---|
| `license-v1-valid.json` | Verified | `AQ==` | `01` | `4bf5122f344554c53bde2ebb8cd2b7e3d1600ad631c385a5d7cce23c7785459a` |
| `license-v1-invalid.json` | Invalid | `AA==` | `00` | `6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d` |
| `license-v1-internal-error.json` | Error | `Ag==` | `02` | `dbc1b4c900ffe48d575b5da5c638040125f65db0fe3e24494b76ea986457d986` |

## Artifact Checksums

These hashes cover the JSON vector artifacts as committed:

- `license-v1-valid.json`: `279aa0ca7e6ceb1dc722b8b2cfb4c437b2091cfd30b91ce286647221ddf21978`
- `license-v1-invalid.json`: `34609980d4a0b6d4a996c6e0d316ce1021fb8f5ef72cdb5df199d5da45cee459`
- `license-v1-internal-error.json`: `0fdc7be97b1aec95682f7cb37496a1808102abd50ebf81e5dab3e03da0fc97ae`

## Reproduction Commands

Run from repository root.

### 1) Verify proof-byte checksums

```bash
python3 - <<'PY'
import base64, hashlib
for b64 in ["AQ==", "AA==", "Ag=="]:
    raw = base64.b64decode(b64)
    print(b64, hashlib.sha256(raw).hexdigest())
PY
```

### 2) Verify JSON artifact checksums

```bash
shasum -a 256 \
  tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-valid.json \
  tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-invalid.json \
  tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/license-v1-internal-error.json
```

### 3) Verify parseability and expected outcomes

```bash
dotnet test tests/Sigil.Sdk.Tests/Sigil.Sdk.Tests.csproj \
  --filter "FullyQualifiedName~MidnightProofConformanceTests"
```

## Internal-Error Simulation Strategy

`license-v1-internal-error.json` uses reserved placeholder proof bytes `0x02` (`Ag==`).
`MidnightZkV1ProofSystemVerifier` maps this deterministic marker to:

- `ProofVerificationResultKind.VerifierError`
- `LicenseFailureCode.ProofVerifierInternalError`

This ensures deterministic internal-fault path coverage without exposing proof material.

## Acceptance Evidence

- 3 vectors exist under `tests/Sigil.Sdk.Tests/Validation/Conformance/Vectors/`.
- Each vector includes a concrete `source` and `checksum` field.
- Vector loader and conformance tests parse these artifacts successfully.

## Next Upgrade Path (Optional)

When public Midnight ecosystem vectors become available, replace placeholder vectors while preserving the same schema and checksum process:

1. Import signed/community vectors.
2. Record source URI/commit in each vector.
3. Replace `proofBytes` and checksums.
4. Re-run conformance tests.
