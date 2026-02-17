// Spec 002 (FR-011): Structured logging helpers (never log proofBytes or raw JSON).

using Microsoft.Extensions.Logging;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Logging;

public static class ValidationLogging
{
    public static void LogValidationResult(ILogger logger, LicenseValidationResult result)
    {
        if (logger is null)
        {
            return;
        }

        try
        {
            logger.LogInformation(
                "Sigil validation completed: {LicenseStatus} {FailureCode} {EnvelopeVersion} {StatementId} {ProofSystem}",
                result.Status,
                result.Failure?.Code,
                result.EnvelopeVersion,
                result.StatementId,
                result.ProofSystem);
        }
        catch
        {
            // Spec 002 (FR-011): logging must not change validation outcomes.
        }
    }

    public static string SanitizeFailureMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Validation failed.";
        }

        // Spec 002: defense-in-depth; do not attempt redaction, just cap length.
        const int maxLen = 512;
        return message.Length <= maxLen ? message : message[..maxLen];
    }
}
