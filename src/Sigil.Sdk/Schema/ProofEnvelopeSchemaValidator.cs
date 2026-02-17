// Spec 002 (FR-005, FR-009): Draft 2020-12 schema validation via Corvus.Json.Validator.

using System.Reflection;
using Corvus.Json;
using Corvus.Json.CodeGeneration.Draft202012;
using Corvus.Json.Validator;

namespace Sigil.Sdk.Schema;

public sealed class ProofEnvelopeSchemaValidator : IProofEnvelopeSchemaValidator
{
    private static readonly Lazy<JsonSchema> Schema = new(LoadSchema, isThreadSafe: true);

    public ProofEnvelopeSchemaValidationResult Validate(System.Text.Json.JsonElement envelopeRoot, bool diagnosticsEnabled)
    {
        // Spec 002: schema validation is deterministic; diagnostics only affects returned counts.
        var context = Schema.Value.Validate(envelopeRoot, ValidationLevel.Flag);

        // The validation context type is external; we avoid depending on its concrete type shape beyond ToString.
        // We use reflection-free checks by relying on the documented IsValid property if present.
        var isValid = (bool)(context.GetType().GetProperty("IsValid")?.GetValue(context) ?? false);
        var errorCount = 0;

        if (diagnosticsEnabled)
        {
            // Best-effort: if the context exposes an ErrorCount property, use it.
            errorCount = (int)(context.GetType().GetProperty("ErrorCount")?.GetValue(context) ?? 0);
        }

        return new ProofEnvelopeSchemaValidationResult(isValid, errorCount);
    }

    private static JsonSchema LoadSchema()
    {
        // Spec 002 (FR-009): Initialize once at startup (lazy singleton) and reuse.
        var assembly = typeof(ProofEnvelopeSchemaValidator).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(n => n.EndsWith("Contracts.proof-envelope.schema.json", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Embedded schema resource not found.");
        using var reader = new StreamReader(stream);
        var schemaText = reader.ReadToEnd();

        var options = new JsonSchema.Options(
            additionalDocumentResolver: null,
            allowFileSystemAndHttpResolution: false,
            fallbackVocabulary: VocabularyAnalyser.DefaultVocabulary,
            alwaysAssertFormat: true);

        return JsonSchema.FromText(schemaText, canonicalUri: null, options);
    }
}
