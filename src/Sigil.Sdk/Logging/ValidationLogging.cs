// Spec 002 (FR-011): Structured logging helpers (never log proofBytes or raw JSON).
// Spec 003 (T023): Respect ValidationOptions logging configuration during validation.

using Microsoft.Extensions.Logging;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.Logging;

public static class ValidationLogging
{
    /// <summary>
    /// Log validation result respecting options configuration.
    /// Spec 003 (T023): When LogFailureDetails is false, sensitive details are omitted from logs.
    /// Spec 002 (FR-011): Never log proofBytes or raw JSON to meet security baseline.
    /// </summary>
    public static void LogValidationResult(
        ILogger logger,
        LicenseValidationResult result,
        ValidationOptions? options = null)
    {
        if (logger is null)
        {
            return;
        }

        try
        {
            var shouldLogDetails = options?.LogFailureDetails ?? false;

            if (result.Status == LicenseStatus.Valid)
            {
                // Log successful validations (no sensitive data)
                logger.LogInformation(
                    "Sigil validation completed: {LicenseStatus} {EnvelopeVersion} {StatementId} {ProofSystem}",
                    result.Status,
                    result.EnvelopeVersion,
                    result.StatementId,
                    result.ProofSystem);
            }
            else if (shouldLogDetails && result.Failure != null)
            {
                // Log with failure details (when enabled)
                logger.LogWarning(
                    "Sigil validation failed: {LicenseStatus} {FailureCode} {FailureMessage} {EnvelopeVersion} {StatementId} {ProofSystem}",
                    result.Status,
                    result.Failure.Code,
                    SanitizeFailureMessage(result.Failure.Message),
                    result.EnvelopeVersion,
                    result.StatementId,
                    result.ProofSystem);
            }
            else
            {
                // Log failures without details (secure default - Spec 003 FR-010)
                logger.LogWarning(
                    "Sigil validation failed: {LicenseStatus} {FailureCode}",
                    result.Status,
                    result.Failure?.Code);
            }
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
