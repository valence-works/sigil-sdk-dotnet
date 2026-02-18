using System.Text;
using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Validation;

public sealed class LicenseValidatorTests
{
    [Fact]
    public async Task ValidateAsync_String_InvalidJson_IsDeterministic_AndNoThrow()
    {
        // Spec 002 (SC-001, SC-002)
        var validator = CreateValidator(schemaValid: true);

        var input = "{";
        var r1 = await validator.ValidateAsync(input);
        var r2 = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Malformed, r1.Status);
        Assert.Equal(LicenseFailureCode.InvalidJson, r1.Failure?.Code);

        Assert.Equal(r1.Status, r2.Status);
        Assert.Equal(r1.Failure?.Code, r2.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_String_SchemaInvalid_ReturnsMalformed()
    {
        // Spec 002 (FR-005)
        var validator = CreateValidator(schemaValid: false);

        var input = "{}";
        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Malformed, result.Status);
        Assert.Equal(LicenseFailureCode.SchemaValidationFailed, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_Stream_ReadFailure_ReturnsDeterministicError()
    {
        // Spec 002 (FR-017a)
        var validator = CreateValidator(schemaValid: true);

        await using var stream = new ThrowingReadStream();
        var result = await validator.ValidateAsync(stream);

        Assert.Equal(LicenseStatus.Error, result.Status);
        Assert.Equal(LicenseFailureCode.StreamReadFailed, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_EmptyRegistries_ReturnsUnsupportedProofSystem()
    {
        // Spec 002 (US3 acceptance scenario)
        var schemaValidator = new FakeSchemaValidator(isValid: true);
        var proofRegistry = new ImmutableProofSystemRegistry(Array.Empty<KeyValuePair<string, IProofSystemVerifier>>());
        var statementRegistry = new ImmutableStatementRegistry(Array.Empty<KeyValuePair<string, IStatementHandler>>());

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true });

        var input = "{"
                    + "\"envelopeVersion\":\"1.0\"," 
                    + "\"proofSystem\":\"test\"," 
                    + "\"statementId\":\"sigil:license\"," 
                    + "\"proofBytes\":\"AA==\"," 
                    + "\"publicInputs\":{"
                    + "\"productId\":\"p\"," 
                    + "\"edition\":\"e\"," 
                    + "\"features\":[\"feature-a\"]"
                    + "}"
                    + "}";

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Unsupported, result.Status);
        Assert.Equal(LicenseFailureCode.UnsupportedProofSystem, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_UnknownStatementId_ReturnsUnsupportedStatement()
    {
        // Spec 002 (US3 acceptance scenario)
        var schemaValidator = new FakeSchemaValidator(isValid: true);
        var verifier = new FakeVerifier(verifyResult: true);

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("test", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(Array.Empty<KeyValuePair<string, IStatementHandler>>());

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true });

        var input = "{"
                    + "\"envelopeVersion\":\"1.0\"," 
                    + "\"proofSystem\":\"test\"," 
                    + "\"statementId\":\"sigil:unknown\"," 
                    + "\"proofBytes\":\"AA==\"," 
                    + "\"publicInputs\":{"
                    + "\"productId\":\"p\"," 
                    + "\"edition\":\"e\"," 
                    + "\"features\":[\"feature-a\"]"
                    + "}"
                    + "}";

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Unsupported, result.Status);
        Assert.Equal(LicenseFailureCode.UnsupportedStatement, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_UnsupportedEnvelopeVersion_ReturnsUnsupportedEnvelopeVersion()
    {
        // Spec 002 (US3 acceptance scenario)
        var schemaValidator = new FakeSchemaValidator(isValid: true);
        var verifier = new FakeVerifier(verifyResult: true);
        var handler = new FakeStatementHandler(isValid: true);

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("test", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>("sigil:license", handler) });

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true });

        var input = "{"
                    + "\"envelopeVersion\":\"2.0\"," 
                    + "\"proofSystem\":\"test\"," 
                    + "\"statementId\":\"sigil:license\"," 
                    + "\"proofBytes\":\"AA==\"," 
                    + "\"publicInputs\":{"
                    + "\"productId\":\"p\"," 
                    + "\"edition\":\"e\"," 
                    + "\"features\":[\"feature-a\"]"
                    + "}"
                    + "}";

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Unsupported, result.Status);
        Assert.Equal(LicenseFailureCode.UnsupportedEnvelopeVersion, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_SchemaInvalid_DoesNotInvokeRegistries()
    {
        // Spec 002 (US2 acceptance scenario)
        var schemaValidator = new FakeSchemaValidator(isValid: false);
        var proofRegistry = new ThrowingProofRegistry();
        var statementRegistry = new ThrowingStatementRegistry();

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true });

        var input = "{}";
        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Malformed, result.Status);
        Assert.Equal(LicenseFailureCode.SchemaValidationFailed, result.Failure?.Code);
    }

    [Fact]
    public async Task ValidateAsync_CustomVerifierAndHandler_AreInvoked()
    {
        // Spec 003 (US3 acceptance scenario)
        var schemaValidator = new FakeSchemaValidator(isValid: true);
        var verifier = new CountingVerifier(verifyResult: true);
        var handler = new CountingStatementHandler(isValid: true);

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("custom", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>("custom", handler) });

        var validator = new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(DateTimeOffset.UnixEpoch),
            new ValidationOptions { EnableDiagnostics = true });

        var input = "{" 
                    + "\"envelopeVersion\":\"1.0\"," 
                    + "\"proofSystem\":\"custom\"," 
                    + "\"statementId\":\"custom\"," 
                    + "\"proofBytes\":\"AA==\"," 
                    + "\"publicInputs\":{}" 
                    + "}";

        var result = await validator.ValidateAsync(input);

        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.Equal(1, verifier.CallCount);
        Assert.Equal(1, handler.CallCount);
    }

    private static LicenseValidator CreateValidator(bool schemaValid)
    {
        var schemaValidator = new FakeSchemaValidator(schemaValid);

        var verifier = new FakeVerifier(verifyResult: true);
        var handler = new FakeStatementHandler(isValid: true);

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("test", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>("test", handler) });

        var clock = new FixedClock(DateTimeOffset.UnixEpoch);
        var options = new ValidationOptions { EnableDiagnostics = true };

        return new LicenseValidator(schemaValidator, proofRegistry, statementRegistry, clock, options);
    }

    private sealed class FakeSchemaValidator : IProofEnvelopeSchemaValidator
    {
        private readonly bool isValid;

        public FakeSchemaValidator(bool isValid)
        {
            this.isValid = isValid;
        }

        public ProofEnvelopeSchemaValidationResult Validate(JsonElement envelopeRoot, bool diagnosticsEnabled)
        {
            return new ProofEnvelopeSchemaValidationResult(isValid, errorCount: isValid ? 0 : 1);
        }
    }

    private sealed class FakeVerifier : IProofSystemVerifier
    {
        private readonly bool verifyResult;

        public FakeVerifier(bool verifyResult)
        {
            this.verifyResult = verifyResult;
        }

        public Task<bool> VerifyAsync(
            string statementId,
            JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(verifyResult);
        }
    }

    private sealed class FakeStatementHandler : IStatementHandler
    {
        private readonly bool isValid;

        public FakeStatementHandler(bool isValid)
        {
            this.isValid = isValid;
        }

        public Task<StatementValidationResult> ValidateAsync(JsonElement publicInputs, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new StatementValidationResult(isValid, claims: isValid ? new LicenseClaims() : null));
        }
    }

    private sealed class CountingVerifier : IProofSystemVerifier
    {
        private readonly bool verifyResult;

        public CountingVerifier(bool verifyResult)
        {
            this.verifyResult = verifyResult;
        }

        public int CallCount { get; private set; }

        public Task<bool> VerifyAsync(
            string statementId,
            JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(verifyResult);
        }
    }

    private sealed class CountingStatementHandler : IStatementHandler
    {
        private readonly bool isValid;

        public CountingStatementHandler(bool isValid)
        {
            this.isValid = isValid;
        }

        public int CallCount { get; private set; }

        public Task<StatementValidationResult> ValidateAsync(JsonElement publicInputs, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new StatementValidationResult(isValid, claims: isValid ? new LicenseClaims() : null));
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

    private sealed class ThrowingReadStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("read failed");
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            throw new IOException("read failed");
        }
    }

    private sealed class ThrowingProofRegistry : IProofSystemRegistry
    {
        public bool TryGetVerifier(string proofSystem, out IProofSystemVerifier verifier)
        {
            throw new InvalidOperationException("Registry should not be invoked when schema is invalid.");
        }
    }

    private sealed class ThrowingStatementRegistry : IStatementRegistry
    {
        public bool TryGetHandler(string statementId, out IStatementHandler handler)
        {
            throw new InvalidOperationException("Registry should not be invoked when schema is invalid.");
        }
    }
}
