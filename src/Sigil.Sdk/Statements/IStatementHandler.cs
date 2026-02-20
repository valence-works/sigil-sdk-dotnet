// Spec 002 (FR-016): Contract for statement semantic validation + claims extraction.

using System.Text.Json;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Statements;

public interface IStatementHandler
{
    /// <summary>
    /// Gets the canonical statement identifier (URN) handled by this instance.
    /// </summary>
    string StatementId { get; }

    Task<StatementValidationResult> ValidateAsync(
        JsonElement publicInputs,
        CancellationToken cancellationToken = default);
}

public sealed class StatementValidationResult
{
    public StatementValidationResult(bool isValid, LicenseClaims? claims)
    {
        IsValid = isValid;
        Claims = claims;
    }

    public bool IsValid { get; }

    public LicenseClaims? Claims { get; }
}
