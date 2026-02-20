using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Validation;

public sealed class LicenseValidatorCryptoTests
{
    [Fact]
    public async Task CryptoFailure_ReturnsInvalid_NotExpired_EvenIfExpiresAtIsPast()
    {
        // Spec 002 (FR-008a, FR-016)
        var validator = CreateValidator(verifierResult: false, handlerValid: true, nowUtc: DateTimeOffset.Parse("2030-01-01T00:00:00Z"), expiresAtUnix: 1);
        var input = EnvelopeJson(expiresAt: 1);

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        Assert.Equal(LicenseFailureCode.ProofVerificationFailed, result.Failure?.Code);
    }

    [Fact]
    public async Task VerifiedExpired_ReturnsExpired_AfterVerification()
    {
        // Spec 002 (FR-008b, SC-003)
        var validator = CreateValidator(verifierResult: true, handlerValid: true, nowUtc: DateTimeOffset.Parse("2030-01-01T00:00:00Z"), expiresAtUnix: 1);
        var input = EnvelopeJson(expiresAt: 1);

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Expired, result.Status);
        Assert.Equal(LicenseFailureCode.LicenseExpired, result.Failure?.Code);
    }

    private static LicenseValidator CreateValidator(bool verifierResult, bool handlerValid, DateTimeOffset nowUtc, long expiresAtUnix)
    {
        var schemaValidator = new AlwaysValidSchemaValidator();

        var verifier = new FakeVerifier(verifierResult);
        var handler = new FakeStatementHandler(handlerValid, expiresAtUnix);

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("test", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>("test", handler) });

        return new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(nowUtc),
            new ValidationOptions { EnableDiagnostics = true });
    }

    private static string EnvelopeJson(long? expiresAt)
    {
        var expiresAtFragment = expiresAt is null ? "" : $", \"expiresAt\": {expiresAt}";
        return "{"
               + "\"envelopeVersion\":\"1.0\"," 
               + "\"proofSystem\":\"test\"," 
               + "\"statementId\":\"test\"," 
               + "\"proofBytes\":\"AA==\"," 
               + $"\"publicInputs\":{{\"subject\":\"x\"{expiresAtFragment}}}"
               + "}";
    }

    private sealed class AlwaysValidSchemaValidator : IProofEnvelopeSchemaValidator
    {
        public ProofEnvelopeSchemaValidationResult Validate(JsonElement envelopeRoot, bool diagnosticsEnabled)
        {
            return new ProofEnvelopeSchemaValidationResult(isValid: true, errorCount: 0);
        }
    }

    private sealed class FakeVerifier : IProofSystemVerifier
    {
        private readonly bool result;

        public FakeVerifier(bool result)
        {
            this.result = result;
        }

        public Task<bool> VerifyAsync(
            string statementId,
            JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }

    private sealed class FakeStatementHandler : IStatementHandler
    {
        private readonly bool isValid;
        private readonly long expiresAtUnix;

        public FakeStatementHandler(bool isValid, long expiresAtUnix)
        {
            this.isValid = isValid;
            this.expiresAtUnix = expiresAtUnix;
        }

        public string StatementId => "test";

        public Task<StatementValidationResult> ValidateAsync(JsonElement publicInputs, CancellationToken cancellationToken = default)
        {
            var claims = isValid
                ? new LicenseClaims("product", "edition", new[] { "feature-a" }, expiresAtUnix, maxSeats: 1)
                : null;
            return Task.FromResult(new StatementValidationResult(isValid, claims));
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
