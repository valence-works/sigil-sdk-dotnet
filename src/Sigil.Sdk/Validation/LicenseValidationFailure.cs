// Spec 002 (FR-004): Failure model for non-Valid outcomes.

namespace Sigil.Sdk.Validation;

public sealed class LicenseValidationFailure
{
    public LicenseValidationFailure(LicenseFailureCode code, string message, Exception? diagnosticException = null)
    {
        Code = code;
        Message = message;
        DiagnosticException = diagnosticException;
    }

    public LicenseFailureCode Code { get; }

    public string Message { get; }

    public Exception? DiagnosticException { get; }
}
