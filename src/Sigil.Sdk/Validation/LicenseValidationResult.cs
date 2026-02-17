// Spec 002 (FR-004, FR-004a): Result-object validation outcome.

namespace Sigil.Sdk.Validation;

public sealed class LicenseValidationResult
{
    public LicenseValidationResult(
        LicenseStatus status,
        string? envelopeVersion,
        string? statementId,
        string? proofSystem,
        LicenseClaims? claims,
        LicenseValidationFailure? failure)
    {
        Status = status;
        EnvelopeVersion = envelopeVersion;
        StatementId = statementId;
        ProofSystem = proofSystem;
        Claims = claims;
        Failure = failure;

        IsValid = Status == LicenseStatus.Valid;

        if (IsValid && Failure is not null)
        {
            throw new ArgumentException("Valid results must not include a failure.", nameof(failure));
        }

        if (!IsValid && Failure is null)
        {
            throw new ArgumentException("Non-valid results must include exactly one failure.", nameof(failure));
        }
    }

    public LicenseStatus Status { get; }

    public bool IsValid { get; }

    public string? EnvelopeVersion { get; }
    public string? StatementId { get; }
    public string? ProofSystem { get; }

    public LicenseClaims? Claims { get; }

    public LicenseValidationFailure? Failure { get; }
}
