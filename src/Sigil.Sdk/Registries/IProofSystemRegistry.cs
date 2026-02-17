// Spec 002 (FR-007): Immutable registry contract for proof system verifiers.

using Sigil.Sdk.Proof;

namespace Sigil.Sdk.Registries;

public interface IProofSystemRegistry
{
    bool TryGetVerifier(string proofSystem, out IProofSystemVerifier verifier);
}
