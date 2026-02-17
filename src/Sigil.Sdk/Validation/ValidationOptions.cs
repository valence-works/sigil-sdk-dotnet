// Spec 002 (FR-011): Diagnostics opt-in.

namespace Sigil.Sdk.Validation;

public sealed class ValidationOptions
{
    public bool EnableDiagnostics { get; init; } = false;
}
