// Spec 002 (FR-005, FR-009): Schema validation abstraction.

namespace Sigil.Sdk.Schema;

public interface IProofEnvelopeSchemaValidator
{
    ProofEnvelopeSchemaValidationResult Validate(System.Text.Json.JsonElement envelopeRoot, bool diagnosticsEnabled);
}

public sealed class ProofEnvelopeSchemaValidationResult
{
    public ProofEnvelopeSchemaValidationResult(bool isValid, int errorCount)
    {
        IsValid = isValid;
        ErrorCount = errorCount;
    }

    public bool IsValid { get; }

    public int ErrorCount { get; }
}
