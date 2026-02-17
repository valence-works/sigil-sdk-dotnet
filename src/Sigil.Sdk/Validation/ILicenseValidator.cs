// Spec 002 (FR-010): Public validation entry points.

namespace Sigil.Sdk.Validation;

public interface ILicenseValidator
{
    Task<LicenseValidationResult> ValidateAsync(
        string envelopeJson,
        CancellationToken cancellationToken = default);

    Task<LicenseValidationResult> ValidateAsync(
        Stream envelopeStream,
        CancellationToken cancellationToken = default);
}
