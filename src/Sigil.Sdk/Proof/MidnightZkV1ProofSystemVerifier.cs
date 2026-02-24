// Spec 005 (FR-004, FR-018): Built-in Midnight proof-system verifier.

using Sigil.Sdk.Statements;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Proof;

public sealed class MidnightZkV1ProofSystemVerifier : IProofSystemVerifier
{
    public Task<ProofVerificationOutcome> VerifyAsync(
        ReadOnlyMemory<byte> proofBytes,
        ProofVerificationContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Deterministic compatibility gate for the built-in v1 statement path.
        if (!string.Equals(context.StatementId, StatementIds.LicenseV1, StringComparison.Ordinal))
        {
            return Task.FromResult(ProofVerificationOutcome.Invalid(LicenseFailureCode.ProofVerificationContextIncompatible));
        }

        if (proofBytes.IsEmpty)
        {
            return Task.FromResult(ProofVerificationOutcome.Invalid(LicenseFailureCode.ProofVerificationFailed));
        }

        // Current SDK slice is verification-only and fail-closed.
        // Until Midnight cryptographic backend integration is provided, deterministically return Invalid.
        return Task.FromResult(ProofVerificationOutcome.Invalid(LicenseFailureCode.ProofVerificationFailed));
    }
}
