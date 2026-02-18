// Spec 002: Deterministic, fail-closed validator implementation.

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sigil.Sdk.Envelope;
using Sigil.Sdk.Logging;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Time;

namespace Sigil.Sdk.Validation;

public sealed class LicenseValidator : ILicenseValidator
{
    private readonly IProofEnvelopeSchemaValidator schemaValidator;
    private readonly IProofSystemRegistry proofSystemRegistry;
    private readonly IStatementRegistry statementRegistry;
    private readonly IClock clock;
    private readonly ValidationOptions options;
    private readonly ILogger? logger;

    public LicenseValidator(
        IProofEnvelopeSchemaValidator schemaValidator,
        IProofSystemRegistry proofSystemRegistry,
        IStatementRegistry statementRegistry,
        IClock clock,
        ValidationOptions options,
        ILogger<LicenseValidator>? logger = null)
    {
        this.schemaValidator = schemaValidator ?? throw new ArgumentNullException(nameof(schemaValidator));
        this.proofSystemRegistry = proofSystemRegistry ?? throw new ArgumentNullException(nameof(proofSystemRegistry));
        this.statementRegistry = statementRegistry ?? throw new ArgumentNullException(nameof(statementRegistry));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    public Task<LicenseValidationResult> ValidateAsync(string envelopeJson, CancellationToken cancellationToken = default)
    {
        // Spec 002 (FR-002a, T025): Programmer error.
        if (envelopeJson is null)
        {
            throw new ArgumentNullException(nameof(envelopeJson));
        }

        return ValidateCoreAsync(
            read: () => Task.FromResult(ProofEnvelopeReader.ReadFromString(envelopeJson)),
            cancellationToken);
    }

    public Task<LicenseValidationResult> ValidateAsync(Stream envelopeStream, CancellationToken cancellationToken = default)
    {
        // Spec 002 (FR-002a, T025): Programmer error.
        if (envelopeStream is null)
        {
            throw new ArgumentNullException(nameof(envelopeStream));
        }

        return ValidateCoreAsync(
            read: async () =>
            {
                try
                {
                    using var ms = new MemoryStream();
                    await envelopeStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    return ProofEnvelopeReader.ReadFromString(json);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is IOException or ObjectDisposedException)
                {
                    throw new StreamReadException("Stream read failed.", ex);
                }
            },
            cancellationToken);
    }

    private async Task<LicenseValidationResult> ValidateCoreAsync(
        Func<Task<ProofEnvelopeReadResult>> read,
        CancellationToken cancellationToken)
    {
        ProofEnvelopeReadResult? readResult = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Stage 1: Parse JSON + early extraction (FR-006).
            readResult = await read().ConfigureAwait(false);

            // Stage 2: Schema validation gate (FR-005).
            var schemaResult = schemaValidator.Validate(readResult.Root, options.EnableDiagnostics);
            if (!schemaResult.IsValid)
            {
                return Fail(LicenseFailureCode.SchemaValidationFailed, readResult, diagnosticException: null);
            }

            // Stage 3: Resolve envelope version + proof system + statement (FR-015).
            if (readResult.EnvelopeVersion is null || readResult.ProofSystem is null || readResult.StatementId is null)
            {
                return Fail(LicenseFailureCode.InternalError, readResult, diagnosticException: null);
            }

            // Spec 001 v1.0 is the only supported envelope version for this SDK slice.
            if (!string.Equals(readResult.EnvelopeVersion, "1.0", StringComparison.Ordinal))
            {
                return Fail(LicenseFailureCode.UnsupportedEnvelopeVersion, readResult, diagnosticException: null);
            }

            if (!proofSystemRegistry.TryGetVerifier(readResult.ProofSystem, out var verifier))
            {
                return Fail(LicenseFailureCode.UnsupportedProofSystem, readResult, diagnosticException: null);
            }

            if (!statementRegistry.TryGetHandler(readResult.StatementId, out var handler))
            {
                return Fail(LicenseFailureCode.UnsupportedStatement, readResult, diagnosticException: null);
            }

            // Stage 4: Extract required fields used post-schema.
            var root = readResult.Root;
            var publicInputs = root.GetProperty("publicInputs");
            var proofBytesBase64 = root.GetProperty("proofBytes").GetString();
            if (proofBytesBase64 is null)
            {
                return Fail(LicenseFailureCode.InternalError, readResult, diagnosticException: null);
            }

            ReadOnlyMemory<byte> proofBytes;
            try
            {
                proofBytes = Convert.FromBase64String(proofBytesBase64);
            }
            catch (FormatException)
            {
                return Fail(LicenseFailureCode.ProofBytesInvalid, readResult, diagnosticException: null);
            }

            // Stage 5: Cryptographic verification.
            var verified = await verifier
                .VerifyAsync(readResult.StatementId, publicInputs, proofBytes, cancellationToken)
                .ConfigureAwait(false);

            if (!verified)
            {
                // Spec 002 (FR-008a): never return Expired if crypto fails.
                return Fail(LicenseFailureCode.ProofVerificationFailed, readResult, diagnosticException: null);
            }

            // Stage 6: Statement semantic validation.
            var statementResult = await handler
                .ValidateAsync(publicInputs, cancellationToken)
                .ConfigureAwait(false);

            if (!statementResult.IsValid)
            {
                return Fail(LicenseFailureCode.StatementValidationFailed, readResult, diagnosticException: null);
            }

            // Stage 7: Expiry evaluation (FR-008) - only after successful verification.
            if (TryGetExpiresAtUtc(publicInputs, out var expiresAtUtc, out var expiresAtPresentButInvalid))
            {
                if (expiresAtUtc < clock.UtcNow)
                {
                    return Fail(LicenseFailureCode.LicenseExpired, readResult, diagnosticException: null);
                }
            }
            else if (expiresAtPresentButInvalid)
            {
                return Fail(LicenseFailureCode.ExpiresAtInvalid, readResult, diagnosticException: null);
            }

            var result = new LicenseValidationResult(
                status: LicenseStatus.Valid,
                envelopeVersion: readResult.EnvelopeVersion,
                statementId: readResult.StatementId,
                proofSystem: readResult.ProofSystem,
                claims: statementResult.Claims,
                failure: null);

            ValidationLogging.LogValidationResult(logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, result, options);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException)
        {
            return Fail(LicenseFailureCode.InvalidJson, readResult, diagnosticException: null);
        }
        catch (StreamReadException ex)
        {
            return Fail(LicenseFailureCode.StreamReadFailed, readResult, ex);
        }
        catch (Exception ex)
        {
            // Spec 002 (FR-012, FR-017): fail-closed + deterministic error.
            return Fail(LicenseFailureCode.InternalError, readResult, ex);
        }
    }

    private LicenseValidationResult Fail(
        LicenseFailureCode code,
        ProofEnvelopeReadResult? readResult,
        Exception? diagnosticException)
    {
        var status = FailureClassification.MapStatus(code);
        var message = ValidationLogging.SanitizeFailureMessage(FailureClassification.DefaultMessage(code));

        var failure = new LicenseValidationFailure(
            code: code,
            message: message,
            diagnosticException: options.EnableDiagnostics ? diagnosticException : null);

        var result = new LicenseValidationResult(
            status: status,
            envelopeVersion: readResult?.EnvelopeVersion,
            statementId: readResult?.StatementId,
            proofSystem: readResult?.ProofSystem,
            claims: null,
            failure: failure);

        ValidationLogging.LogValidationResult(logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, result, options);
        return result;
    }

    private static bool TryGetExpiresAtUtc(JsonElement publicInputs, out DateTimeOffset expiresAtUtc, out bool presentButInvalid)
    {
        expiresAtUtc = default;
        presentButInvalid = false;

        if (publicInputs.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!publicInputs.TryGetProperty("expiresAt", out var expiresAtProp))
        {
            return false;
        }

        if (expiresAtProp.ValueKind != JsonValueKind.String)
        {
            presentButInvalid = true;
            return false;
        }

        var s = expiresAtProp.GetString();
        if (string.IsNullOrWhiteSpace(s))
        {
            presentButInvalid = true;
            return false;
        }

        var parsed = DateTimeOffset.TryParse(
            s,
            null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out expiresAtUtc);

        presentButInvalid = !parsed;
        return parsed;
    }

    private sealed class StreamReadException : Exception
    {
        public StreamReadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
