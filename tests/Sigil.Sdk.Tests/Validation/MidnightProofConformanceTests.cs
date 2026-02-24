using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Tests.Validation.Conformance;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Validation;

public sealed class MidnightProofConformanceTests
{
    [Fact]
    public void ConformanceVectors_MinimumCoverage_IsPresent()
    {
        var (isValid, missingVectors) = MidnightConformanceVectors.ValidateRequiredVectors();

        Assert.True(isValid, $"Missing required vectors: {string.Join(", ", missingVectors)}");
    }

    [Fact]
    public async Task KnownValidVector_LicenseV1_ReturnsVerified()
    {
        var vector = MidnightConformanceVectors.GetFirstByOutcome("Verified");
        Assert.NotNull(vector);

        var verifier = new MidnightZkV1ProofSystemVerifier();
        var proofBytes = Convert.FromBase64String(vector!.ProofBytes);
        var context = new ProofVerificationContext(vector.StatementId, CreatePayload("product-a", 100));

        var outcome = await verifier.VerifyAsync(proofBytes, context);

        Assert.Equal(ProofVerificationResultKind.Verified, outcome.Kind);
        Assert.Null(outcome.FailureCode);
    }

    [Fact]
    public async Task KnownInvalidVector_LicenseV1_ReturnsInvalid()
    {
        var vector = MidnightConformanceVectors.GetFirstByOutcome("Invalid");
        Assert.NotNull(vector);

        var verifier = new MidnightZkV1ProofSystemVerifier();
        var proofBytes = Convert.FromBase64String(vector!.ProofBytes);
        var context = new ProofVerificationContext(vector.StatementId, CreatePayload("product-a", 100));

        var outcome = await verifier.VerifyAsync(proofBytes, context);

        Assert.Equal(ProofVerificationResultKind.InvalidProof, outcome.Kind);
        Assert.Equal(LicenseFailureCode.ProofVerificationFailed, outcome.FailureCode);
    }

    [Fact]
    public async Task InternalErrorVector_LicenseV1_ReturnsVerifierError_Deterministically()
    {
        var vector = MidnightConformanceVectors.GetFirstByOutcome("Error");
        Assert.NotNull(vector);

        var verifier = new MidnightZkV1ProofSystemVerifier();
        var proofBytes = Convert.FromBase64String(vector!.ProofBytes);
        var context = new ProofVerificationContext(vector.StatementId, CreatePayload("product-a", 100));

        var first = await verifier.VerifyAsync(proofBytes, context);
        var second = await verifier.VerifyAsync(proofBytes, context);

        Assert.Equal(ProofVerificationResultKind.VerifierError, first.Kind);
        Assert.Equal(LicenseFailureCode.ProofVerifierInternalError, first.FailureCode);
        Assert.Equal(first.Kind, second.Kind);
        Assert.Equal(first.FailureCode, second.FailureCode);
    }

    [Fact]
    public async Task Verification_IsStatementBound_NotClaimSemanticsBound()
    {
        var vector = MidnightConformanceVectors.GetFirstByOutcome("Verified");
        Assert.NotNull(vector);

        var verifier = new MidnightZkV1ProofSystemVerifier();
        var proofBytes = Convert.FromBase64String(vector!.ProofBytes);

        var contextA = new ProofVerificationContext(StatementIds.LicenseV1, CreatePayload("product-a", 10));
        var contextB = new ProofVerificationContext(StatementIds.LicenseV1, CreatePayload("product-b", 2000));

        var outcomeA = await verifier.VerifyAsync(proofBytes, contextA);
        var outcomeB = await verifier.VerifyAsync(proofBytes, contextB);

        Assert.Equal(outcomeA.Kind, outcomeB.Kind);
        Assert.Equal(outcomeA.FailureCode, outcomeB.FailureCode);
    }

    [Fact]
    public async Task Verification_RejectsUnsupportedStatementContext_Deterministically()
    {
        var verifier = new MidnightZkV1ProofSystemVerifier();
        var proofBytes = Convert.FromBase64String("AQ==");

        var unsupportedContext = new ProofVerificationContext(
            "urn:sigil:statement:other:v1",
            CreatePayload("product-a", 10));

        var outcome = await verifier.VerifyAsync(proofBytes, unsupportedContext);

        Assert.Equal(ProofVerificationResultKind.InvalidProof, outcome.Kind);
        Assert.Equal(LicenseFailureCode.ProofVerificationContextIncompatible, outcome.FailureCode);
    }

    private static JsonElement CreatePayload(string productId, int maxSeats)
    {
        using var document = JsonDocument.Parse($"{{\"productId\":\"{productId}\",\"maxSeats\":{maxSeats}}}");
        return document.RootElement.Clone();
    }
}