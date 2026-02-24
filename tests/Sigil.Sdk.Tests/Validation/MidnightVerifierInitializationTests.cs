using System.Collections.Concurrent;
using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Validation;

public sealed class MidnightVerifierInitializationTests
{
    [Fact]
    public async Task RepeatedVerifications_AreDeterministic_AfterInitialization()
    {
        using var verifier = new MidnightZkV1ProofSystemVerifier();
        var context = CreateLicenseV1Context();
        var proofBytes = Convert.FromBase64String("AQ==");

        var outcomes = new List<ProofVerificationOutcome>();
        for (var i = 0; i < 100; i++)
        {
            outcomes.Add(await verifier.VerifyAsync(proofBytes, context));
        }

        Assert.All(outcomes, o => Assert.Equal(ProofVerificationResultKind.Verified, o.Kind));
        Assert.All(outcomes, o => Assert.Null(o.FailureCode));
    }

    [Fact]
    public async Task ConcurrentVerifications_AreDeterministic_AndThreadSafe()
    {
        using var verifier = new MidnightZkV1ProofSystemVerifier();
        var context = CreateLicenseV1Context();
        var proofBytes = Convert.FromBase64String("AQ==");

        var results = new ConcurrentBag<ProofVerificationOutcome>();
        var tasks = Enumerable.Range(0, 128)
            .Select(_ => Task.Run(async () =>
            {
                var outcome = await verifier.VerifyAsync(proofBytes, context);
                results.Add(outcome);
            }));

        await Task.WhenAll(tasks);

        Assert.Equal(128, results.Count);
        Assert.All(results, o => Assert.Equal(ProofVerificationResultKind.Verified, o.Kind));
        Assert.All(results, o => Assert.Null(o.FailureCode));
    }

    [Fact]
    public async Task ConcurrentMixedProofs_ProduceExpectedOutcomes_WithoutRaces()
    {
        using var verifier = new MidnightZkV1ProofSystemVerifier();
        var context = CreateLicenseV1Context();

        var validProof = Convert.FromBase64String("AQ==");
        var invalidProof = Convert.FromBase64String("AA==");

        var results = new ConcurrentBag<ProofVerificationOutcome>();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(async () =>
            {
                var proof = i % 2 == 0 ? validProof : invalidProof;
                var outcome = await verifier.VerifyAsync(proof, context);
                results.Add(outcome);
            }));

        await Task.WhenAll(tasks);

        var verifiedCount = results.Count(r => r.Kind == ProofVerificationResultKind.Verified);
        var invalidCount = results.Count(r => r.Kind == ProofVerificationResultKind.InvalidProof);
        var errorCount = results.Count(r => r.Kind == ProofVerificationResultKind.VerifierError);

        Assert.Equal(100, results.Count);
        Assert.Equal(50, verifiedCount);
        Assert.Equal(50, invalidCount);
        Assert.Equal(0, errorCount);
    }

    private static ProofVerificationContext CreateLicenseV1Context()
    {
        using var document = JsonDocument.Parse("{\"productId\":\"product\",\"maxSeats\":10}");
        return new ProofVerificationContext(StatementIds.LicenseV1, document.RootElement.Clone());
    }
}
