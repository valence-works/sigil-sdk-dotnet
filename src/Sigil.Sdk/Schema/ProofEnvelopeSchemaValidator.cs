// Spec 002 (FR-005, FR-009): Runtime-safe proof envelope schema validation.

using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sigil.Sdk.Schema;

public sealed class ProofEnvelopeSchemaValidator : IProofEnvelopeSchemaValidator
{
    private static readonly Regex StatementIdPattern = new("^[a-zA-Z][a-zA-Z0-9+.-]*:.*$", RegexOptions.Compiled);
    private static readonly Regex ProofBytesPattern = new("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$", RegexOptions.Compiled);
    private static readonly Regex FeaturePattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly HashSet<string> PublicInputFields = new(StringComparer.Ordinal)
    {
        "productId",
        "edition",
        "features",
        "expiresAt",
        "maxSeats"
    };

    public ProofEnvelopeSchemaValidationResult Validate(JsonElement envelopeRoot, bool diagnosticsEnabled)
    {
        var errorCount = CountErrors(envelopeRoot, diagnosticsEnabled);
        return new ProofEnvelopeSchemaValidationResult(errorCount == 0, diagnosticsEnabled ? errorCount : 0);
    }

    private static int CountErrors(JsonElement root, bool diagnosticsEnabled)
    {
        var errors = 0;
        AddErrorIf(root.ValueKind != JsonValueKind.Object, ref errors, diagnosticsEnabled);
        if (errors > 0 && !diagnosticsEnabled)
        {
            return errors;
        }

        RequireString(root, "envelopeVersion", value => value == "1.0", ref errors, diagnosticsEnabled);
        RequireString(root, "proofSystem", value => !string.IsNullOrWhiteSpace(value), ref errors, diagnosticsEnabled);
        RequireString(root, "statementId", value => StatementIdPattern.IsMatch(value), ref errors, diagnosticsEnabled);
        RequireString(root, "proofBytes", value => value.Length > 0 && ProofBytesPattern.IsMatch(value), ref errors, diagnosticsEnabled);

        if (root.TryGetProperty("issuedAt", out var issuedAt))
        {
            AddErrorIf(
                issuedAt.ValueKind != JsonValueKind.String
                    || !DateTimeOffset.TryParse(issuedAt.GetString(), out _),
                ref errors,
                diagnosticsEnabled);
        }

        if (root.TryGetProperty("policyHash", out var policyHash))
        {
            AddErrorIf(
                policyHash.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(policyHash.GetString()),
                ref errors,
                diagnosticsEnabled);
        }

        if (root.TryGetProperty("extensions", out var extensions))
        {
            AddErrorIf(extensions.ValueKind != JsonValueKind.Object, ref errors, diagnosticsEnabled);
        }

        if (!root.TryGetProperty("publicInputs", out var publicInputs))
        {
            AddError(ref errors, diagnosticsEnabled);
            return errors;
        }

        ValidatePublicInputs(publicInputs, ref errors, diagnosticsEnabled);
        return errors;
    }

    private static void ValidatePublicInputs(JsonElement publicInputs, ref int errors, bool diagnosticsEnabled)
    {
        AddErrorIf(publicInputs.ValueKind != JsonValueKind.Object, ref errors, diagnosticsEnabled);
        if (publicInputs.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in publicInputs.EnumerateObject())
        {
            AddErrorIf(!PublicInputFields.Contains(property.Name), ref errors, diagnosticsEnabled);
        }

        RequireString(publicInputs, "productId", value => !string.IsNullOrWhiteSpace(value), ref errors, diagnosticsEnabled);
        RequireString(publicInputs, "edition", value => !string.IsNullOrWhiteSpace(value), ref errors, diagnosticsEnabled);
        ValidateFeatures(publicInputs, ref errors, diagnosticsEnabled);
        ValidateOptionalPositiveInteger(publicInputs, "expiresAt", ref errors, diagnosticsEnabled);
        ValidateOptionalPositiveInteger(publicInputs, "maxSeats", ref errors, diagnosticsEnabled);
    }

    private static void ValidateFeatures(JsonElement publicInputs, ref int errors, bool diagnosticsEnabled)
    {
        if (!publicInputs.TryGetProperty("features", out var features))
        {
            AddError(ref errors, diagnosticsEnabled);
            return;
        }

        if (features.ValueKind != JsonValueKind.Array)
        {
            AddError(ref errors, diagnosticsEnabled);
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var feature in features.EnumerateArray())
        {
            var isValidFeature = feature.ValueKind == JsonValueKind.String
                && feature.GetString() is { } value
                && value.Length > 0
                && FeaturePattern.IsMatch(value)
                && seen.Add(value);

            AddErrorIf(!isValidFeature, ref errors, diagnosticsEnabled);
        }
    }

    private static void ValidateOptionalPositiveInteger(JsonElement root, string propertyName, ref int errors, bool diagnosticsEnabled)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return;
        }

        AddErrorIf(property.ValueKind != JsonValueKind.Number || !property.TryGetInt64(out var value) || value < 1, ref errors, diagnosticsEnabled);
    }

    private static void RequireString(JsonElement root, string propertyName, Func<string, bool> predicate, ref int errors, bool diagnosticsEnabled)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String
            || property.GetString() is not { } value
            || !predicate(value))
        {
            AddError(ref errors, diagnosticsEnabled);
        }
    }

    private static void AddErrorIf(bool condition, ref int errors, bool diagnosticsEnabled)
    {
        if (condition)
        {
            AddError(ref errors, diagnosticsEnabled);
        }
    }

    private static void AddError(ref int errors, bool diagnosticsEnabled)
    {
        errors++;
        if (!diagnosticsEnabled)
        {
            return;
        }
    }
}
