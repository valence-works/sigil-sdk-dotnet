// Spec 002 (FR-013): Deterministic, stable failure codes (explicit numeric values).

namespace Sigil.Sdk.Validation;

public enum LicenseFailureCode
{
    InvalidJson = 1,
    SchemaValidationFailed = 2,
    UnsupportedEnvelopeVersion = 3,
    UnsupportedProofSystem = 4,
    UnsupportedStatement = 5,
    ProofBytesInvalid = 6,
    ProofVerificationFailed = 7,
    LicenseExpired = 8,
    StreamReadFailed = 9,
    StatementValidationFailed = 10,
    InternalError = 11,
    ExpiresAtInvalid = 12,
}
