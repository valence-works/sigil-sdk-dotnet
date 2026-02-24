// Spec 002 (FR-013, FR-014..FR-017): Deterministic failure classification.

namespace Sigil.Sdk.Validation;

public static class FailureClassification
{
    static FailureClassification()
    {
        System.Diagnostics.Debug.Assert(
            MapStatus(LicenseFailureCode.LicenseExpired) == LicenseStatus.Expired,
            "LicenseExpired must map to LicenseStatus.Expired.");
    }

    public static LicenseStatus MapStatus(LicenseFailureCode code)
    {
        return code switch
        {
            LicenseFailureCode.InvalidJson => LicenseStatus.Malformed,
            LicenseFailureCode.SchemaValidationFailed => LicenseStatus.Malformed,
            LicenseFailureCode.ProofBytesInvalid => LicenseStatus.Malformed,

            LicenseFailureCode.UnsupportedEnvelopeVersion => LicenseStatus.Unsupported,
            LicenseFailureCode.UnsupportedProofSystem => LicenseStatus.Unsupported,
            LicenseFailureCode.UnsupportedStatement => LicenseStatus.Unsupported,

            LicenseFailureCode.ProofVerificationFailed => LicenseStatus.Invalid,
            LicenseFailureCode.ProofVerificationContextIncompatible => LicenseStatus.Invalid,
            // StatementValidationFailed covers semantic validation and statement contract violations.
            LicenseFailureCode.StatementValidationFailed => LicenseStatus.Invalid,

            LicenseFailureCode.LicenseExpired => LicenseStatus.Expired,
            LicenseFailureCode.ExpiresAtInvalid => LicenseStatus.Malformed,

            LicenseFailureCode.StreamReadFailed => LicenseStatus.Error,
            LicenseFailureCode.InternalError => LicenseStatus.Error,
            LicenseFailureCode.ProofVerifierInternalError => LicenseStatus.Error,

            _ => LicenseStatus.Error,
        };
    }

    public static string DefaultMessage(LicenseFailureCode code)
    {
        return code switch
        {
            LicenseFailureCode.InvalidJson => "Input is not valid JSON.",
            LicenseFailureCode.SchemaValidationFailed => "Input does not conform to the Proof Envelope schema.",
            LicenseFailureCode.UnsupportedEnvelopeVersion => "Envelope version is not supported.",
            LicenseFailureCode.UnsupportedProofSystem => "Proof system is not supported.",
            LicenseFailureCode.UnsupportedStatement => "Statement is not supported.",
            LicenseFailureCode.ProofBytesInvalid => "proofBytes is not valid base64.",
            LicenseFailureCode.ProofVerificationFailed => "Cryptographic verification failed.",
            LicenseFailureCode.ProofVerificationContextIncompatible => "Proof verification context is incompatible.",
            LicenseFailureCode.StatementValidationFailed => "Statement validation failed.",
            LicenseFailureCode.LicenseExpired => "License is expired.",
            LicenseFailureCode.ExpiresAtInvalid => "expiresAt is not a valid unix timestamp.",
            LicenseFailureCode.StreamReadFailed => "Failed to read input stream.",
            LicenseFailureCode.ProofVerifierInternalError => "Proof verifier internal error.",
            _ => "Internal validation error.",
        };
    }
}
