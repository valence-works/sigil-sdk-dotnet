// Spec 005 (FR-001, FR-002): Contract for proof-system verification.

namespace Sigil.Sdk.Proof;

public interface IProofSystemVerifier
{
    Task<ProofVerificationOutcome> VerifyAsync(
        ReadOnlyMemory<byte> proofBytes,
        ProofVerificationContext context,
        CancellationToken cancellationToken = default);
}
