using Microsoft.Extensions.Logging;
using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Logging;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Logging;

public sealed class ValidationLoggingTests
{
    /// <summary>
    /// Spec 002 (SC-005): Never log proofBytes or raw JSON
    /// </summary>
    [Fact]
    public void LogValidationResult_NeverLogsProofBytesOrRawJson()
    {
        // Spec 002 (SC-005)
        var logger = new CapturingLogger();
        var fakeProofBytes = "VGhpcy1pcy1ub3QtcHJvb2ZCeXRlcw==";

        var result = new LicenseValidationResult(
            status: LicenseStatus.Malformed,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: null,
            failure: new LicenseValidationFailure(
                LicenseFailureCode.SchemaValidationFailed,
                message: $"bad input {fakeProofBytes}",
                diagnosticException: null));

        ValidationLogging.LogValidationResult(logger, result);

        var combined = string.Join("\n", logger.Messages);
        Assert.DoesNotContain("proofBytes", combined);
        Assert.DoesNotContain(fakeProofBytes, combined);
        Assert.DoesNotContain("{\"", combined); // crude guard against raw JSON blobs
    }

    [Fact]
    public async Task Validator_DiagnosticsRedactProofMaterial_ForVerifierInternalError()
    {
        var fakeProofBytes = "VGhpcy1zaG91bGQtbm90LWxlYWstdmlhLWRpYWdub3N0aWNz";
        var schemaValidator = new AlwaysValidSchemaValidator();
        var verifier = new ThrowingVerifier(fakeProofBytes);
        var statementHandler = new AlwaysValidStatementHandler();

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("midnight-zk-v1", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>(StatementIds.LicenseV1, statementHandler) });

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true, LogFailureDetails = true });

        var envelopeJson = "{" +
                           "\"envelopeVersion\":\"1.0\"," +
                           "\"proofSystem\":\"midnight-zk-v1\"," +
                           "\"statementId\":\"urn:sigil:statement:license:v1\"," +
                           "\"proofBytes\":\"AA==\"," +
                           "\"publicInputs\":{\"subject\":\"x\"}" +
                           "}";

        var result = await validator.ValidateAsync(envelopeJson);

        Assert.Equal(LicenseStatus.Error, result.Status);
        Assert.Equal(LicenseFailureCode.ProofVerifierInternalError, result.Failure?.Code);
        Assert.NotNull(result.Failure?.DiagnosticException);
        Assert.DoesNotContain(fakeProofBytes, result.Failure!.DiagnosticException!.Message);
        Assert.Equal("Validation internal error.", result.Failure.DiagnosticException.Message);
    }

    /// <summary>
    /// Spec 003 (T027): When LogFailureDetails=false, failure details not logged (secure default)
    /// </summary>
    [Fact]
    public void LogValidationResult_WithLogFailureDetailsFalse_OmitsFailureMessage()
    {
        // Arrange
        var logger = new CapturingLogger();
        var options = new ValidationOptions { LogFailureDetails = false };

        var result = new LicenseValidationResult(
            status: LicenseStatus.Invalid,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: null,
            failure: new LicenseValidationFailure(
                LicenseFailureCode.ProofVerificationFailed,
                message: "Verification failed due to invalid signature",
                diagnosticException: null));

        // Act
        ValidationLogging.LogValidationResult(logger, result, options);

        // Assert - Failure code logged but not message
        var combined = string.Join("\n", logger.Messages);
        Assert.Contains("ProofVerificationFailed", combined);
        Assert.DoesNotContain("Verification failed", combined);
        Assert.DoesNotContain("invalid signature", combined);
    }

    /// <summary>
    /// Spec 005 (FR-016/FR-016a): When LogFailureDetails=true, only approved metadata is logged
    /// </summary>
    [Fact]
    public void LogValidationResult_WithLogFailureDetailsTrue_LogsMetadataOnly()
    {
        // Arrange
        var logger = new CapturingLogger();
        var options = new ValidationOptions { LogFailureDetails = true };

        var result = new LicenseValidationResult(
            status: LicenseStatus.Invalid,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: null,
            failure: new LicenseValidationFailure(
                LicenseFailureCode.ProofVerificationFailed,
                message: "Verification failed due to invalid signature",
                diagnosticException: null));

        // Act
        ValidationLogging.LogValidationResult(logger, result, options);

        // Assert - Failure code and metadata logged, but no raw failure message
        var combined = string.Join("\n", logger.Messages);
        Assert.Contains("ProofVerificationFailed", combined);
        Assert.Contains("stmt", combined);
        Assert.Contains("ps", combined);
        Assert.DoesNotContain("invalid signature", combined);
    }

    /// <summary>
    /// Spec 003 (T023): LogFailureDetails defaults to false (secure default)
    /// </summary>
    [Fact]
    public void LogValidationResult_WithNoOptions_DefaultsToSecureLogging()
    {
        // Arrange - No options provided
        var logger = new CapturingLogger();

        var result = new LicenseValidationResult(
            status: LicenseStatus.Invalid,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: null,
            failure: new LicenseValidationFailure(
                LicenseFailureCode.ProofVerificationFailed,
                message: "Verification failed due to invalid signature",
                diagnosticException: null));

        // Act - Pass null options
        ValidationLogging.LogValidationResult(logger, result, options: null);

        // Assert - Failure code logged but not sensitive message (secure default)
        var combined = string.Join("\n", logger.Messages);
        Assert.Contains("ProofVerificationFailed", combined);
        Assert.DoesNotContain("invalid signature", combined);
    }

    /// <summary>
    /// Spec 003 (T023): Successful validations logged with status
    /// </summary>
    [Fact]
    public void LogValidationResult_WithValidStatus_LogsSuccess()
    {
        // Arrange
        var logger = new CapturingLogger();
        var options = new ValidationOptions { LogFailureDetails = false };

        var result = new LicenseValidationResult(
            status: LicenseStatus.Valid,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: new LicenseClaims(
                productId: "product",
                edition: "edition",
                features: new[] { "feature-a" },
                expiresAt: 1,
                maxSeats: 1,
                issuedAt: 1),
            failure: null);

        // Act
        ValidationLogging.LogValidationResult(logger, result, options);

        // Assert - Success and metadata logged
        var combined = string.Join("\n", logger.Messages);
        Assert.Contains("Valid", combined);
        Assert.Contains("stmt", combined);
        Assert.Contains("ps", combined);
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NullScope();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    private sealed class AlwaysValidSchemaValidator : IProofEnvelopeSchemaValidator
    {
        public ProofEnvelopeSchemaValidationResult Validate(JsonElement envelopeRoot, bool diagnosticsEnabled)
        {
            return new ProofEnvelopeSchemaValidationResult(isValid: true, errorCount: 0);
        }
    }

    private sealed class ThrowingVerifier : IProofSystemVerifier
    {
        private readonly string proofMaterial;

        public ThrowingVerifier(string proofMaterial)
        {
            this.proofMaterial = proofMaterial;
        }

        public Task<ProofVerificationOutcome> VerifyAsync(
            ReadOnlyMemory<byte> proofBytes,
            ProofVerificationContext context,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException($"internal verifier fault: {proofMaterial}");
        }
    }

    private sealed class AlwaysValidStatementHandler : IStatementHandler
    {
        public string StatementId => StatementIds.LicenseV1;

        public Task<StatementValidationResult> ValidateAsync(JsonElement publicInputs, CancellationToken cancellationToken = default)
        {
            var claims = new LicenseClaims(
                productId: "product",
                edition: "edition",
                features: new[] { "feature-a" },
                expiresAt: 60 * 60 * 24 * 365,
                maxSeats: 1,
                issuedAt: 1);

            return Task.FromResult(new StatementValidationResult(isValid: true, claims: claims));
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
