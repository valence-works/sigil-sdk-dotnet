// Spec 002 (FR-016): Contract for cryptographic verification.

using System.Text.Json;

namespace Sigil.Sdk.Proof;

public interface IProofSystemVerifier
{
    Task<bool> VerifyAsync(
        string statementId,
        JsonElement publicInputs,
        ReadOnlyMemory<byte> proofBytes,
        CancellationToken cancellationToken = default);
}
