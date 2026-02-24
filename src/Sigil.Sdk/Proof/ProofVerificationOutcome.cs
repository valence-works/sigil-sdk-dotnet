// Spec 005 (FR-005, FR-010, FR-011): Deterministic verifier outcome model.

using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Proof;

public enum ProofVerificationResultKind
{
    Verified,
    InvalidProof,
    VerifierError,
}

public sealed class ProofVerificationOutcome
{
    private ProofVerificationOutcome(
        ProofVerificationResultKind kind,
        LicenseFailureCode? failureCode,
        Exception? diagnosticException)
    {
        Kind = kind;
        FailureCode = failureCode;
        DiagnosticException = diagnosticException;
    }

    public ProofVerificationResultKind Kind { get; }

    public LicenseFailureCode? FailureCode { get; }

    public Exception? DiagnosticException { get; }

    public static ProofVerificationOutcome Verified() =>
        new(ProofVerificationResultKind.Verified, null, null);

    public static ProofVerificationOutcome Invalid(
        LicenseFailureCode failureCode = LicenseFailureCode.ProofVerificationFailed) =>
        new(ProofVerificationResultKind.InvalidProof, failureCode, null);

    public static ProofVerificationOutcome VerifierError(
        LicenseFailureCode failureCode = LicenseFailureCode.ProofVerifierInternalError,
        Exception? diagnosticException = null) =>
        new(ProofVerificationResultKind.VerifierError, failureCode, diagnosticException);
}
