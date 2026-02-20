// Spec 004: Built-in statement handler for license:v1.

using System.Text.Json;
using System.Text.RegularExpressions;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Statements;

public sealed class LicenseV1StatementHandler : IStatementHandler
{
    private static readonly Regex FeaturePattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedFields = new(StringComparer.Ordinal)
    {
        "productId",
        "edition",
        "features",
        "expiresAt",
        "maxSeats",
        "issuedAt",
        "metadata",
    };

    public string StatementId => StatementIds.LicenseV1;

    public Task<StatementValidationResult> ValidateAsync(
        JsonElement publicInputs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (publicInputs.ValueKind != JsonValueKind.Object)
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        foreach (var prop in publicInputs.EnumerateObject())
        {
            if (!AllowedFields.Contains(prop.Name))
            {
                return Task.FromResult(new StatementValidationResult(false, null));
            }
        }

        if (!TryGetNonEmptyString(publicInputs, "productId", out var productId))
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        if (!TryGetNonEmptyString(publicInputs, "edition", out var edition))
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        if (!TryGetFeatures(publicInputs, out var features, cancellationToken))
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        if (!TryGetUnixSeconds(publicInputs, "expiresAt", out var expiresAt))
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        if (!TryGetPositiveInt(publicInputs, "maxSeats", out var maxSeats))
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        long? issuedAt = null;
        if (publicInputs.TryGetProperty("issuedAt", out var issuedAtProp))
        {
            if (!TryGetUnixSecondsValue(issuedAtProp, out var issuedAtValue))
            {
                return Task.FromResult(new StatementValidationResult(false, null));
            }

            issuedAt = issuedAtValue;
        }

        if (publicInputs.TryGetProperty("metadata", out var metadataProp) && metadataProp.ValueKind != JsonValueKind.Object)
        {
            return Task.FromResult(new StatementValidationResult(false, null));
        }

        var claims = new LicenseClaims(
            productId: productId,
            edition: edition,
            features: features,
            expiresAt: expiresAt,
            maxSeats: maxSeats,
            issuedAt: issuedAt);

        return Task.FromResult(new StatementValidationResult(true, claims));
    }

    private static bool TryGetNonEmptyString(JsonElement publicInputs, string name, out string value)
    {
        value = string.Empty;

        if (!publicInputs.TryGetProperty(name, out var prop))
        {
            return false;
        }

        if (prop.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var s = prop.GetString();
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        value = s;
        return true;
    }

    private static bool TryGetFeatures(JsonElement publicInputs, out IReadOnlyList<string> features, CancellationToken cancellationToken)
    {
        features = Array.Empty<string>();

        if (!publicInputs.TryGetProperty("features", out var prop))
        {
            return false;
        }

        if (prop.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var values = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in prop.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var value = item.GetString();
            if (string.IsNullOrWhiteSpace(value) || !FeaturePattern.IsMatch(value))
            {
                return false;
            }

            if (!seen.Add(value))
            {
                return false;
            }

            values.Add(value);
        }

        features = values;
        return true;
    }

    private static bool TryGetPositiveInt(JsonElement publicInputs, string name, out int value)
    {
        value = 0;

        if (!publicInputs.TryGetProperty(name, out var prop))
        {
            return false;
        }

        if (prop.ValueKind != JsonValueKind.Number || !prop.TryGetInt32(out value))
        {
            return false;
        }

        return value > 0;
    }

    private static bool TryGetUnixSeconds(JsonElement publicInputs, string name, out long value)
    {
        value = 0;

        if (!publicInputs.TryGetProperty(name, out var prop))
        {
            return false;
        }

        return TryGetUnixSecondsValue(prop, out value);
    }

    private static bool TryGetUnixSecondsValue(JsonElement prop, out long value)
    {
        value = 0;

        if (prop.ValueKind != JsonValueKind.Number || !prop.TryGetInt64(out value))
        {
            return false;
        }

        return value > 0;
    }
}
