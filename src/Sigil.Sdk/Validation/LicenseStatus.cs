// Spec 002 (FR-003): Public license status model.

namespace Sigil.Sdk.Validation;

public enum LicenseStatus
{
    Valid,
    Invalid,
    Expired,
    Unsupported,
    Malformed,
    Error,
}
