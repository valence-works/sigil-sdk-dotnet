// Spec 005 (FR-002): Proof verification context passed to proof-system verifiers.

using System.Text.Json;

namespace Sigil.Sdk.Proof;

public sealed class ProofVerificationContext
{
    public ProofVerificationContext(string statementId, JsonElement contextPayload)
    {
        if (string.IsNullOrWhiteSpace(statementId))
        {
            throw new ArgumentException("StatementId cannot be null or whitespace.", nameof(statementId));
        }

        StatementId = statementId;
        ContextPayload = contextPayload;
    }

    public string StatementId { get; }

    public JsonElement ContextPayload { get; }
}
