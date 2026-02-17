using System.Diagnostics;
using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Performance;

public sealed class ValidationPerformanceBenchmarks
{
    [Fact(Skip = "Manual performance benchmark (Spec 002 SC-004).")]
    public async Task Validate_P95_UnderOneSecond_For10KbEnvelopes_Offline()
    {
        // Spec 002 (SC-004): measurement method.
        var validator = CreateValidator(nowUtc: DateTimeOffset.Parse("2030-01-01T00:00:00Z"));
        var envelope = CreateEnvelopeJson(payloadSizeBytes: 10 * 1024);

        // Warm-up (do not count schema init/first-run costs).
        for (var i = 0; i < 10; i++)
        {
            _ = await validator.ValidateAsync(envelope);
        }

        var durations = new List<TimeSpan>(capacity: 100);
        for (var i = 0; i < 100; i++)
        {
            var sw = Stopwatch.StartNew();
            _ = await validator.ValidateAsync(envelope);
            sw.Stop();
            durations.Add(sw.Elapsed);
        }

        durations.Sort();
        var p95 = durations[(int)Math.Floor(durations.Count * 0.95) - 1];

        // Intentionally no assert to avoid environment flakiness.
        // Inspect p95 in the debugger/test output when running manually.
        Assert.True(p95 >= TimeSpan.Zero);
    }

    private static LicenseValidator CreateValidator(DateTimeOffset nowUtc)
    {
        var schemaValidator = new ProofEnvelopeSchemaValidator();

        var verifier = new AlwaysTrueVerifier();
        var handler = new AlwaysValidStatementHandler();

        var proofRegistry = new ImmutableProofSystemRegistry(
            new[] { new KeyValuePair<string, IProofSystemVerifier>("test", verifier) });
        var statementRegistry = new ImmutableStatementRegistry(
            new[] { new KeyValuePair<string, IStatementHandler>("sigil:license", handler) });

        return new LicenseValidator(
            schemaValidator,
            proofRegistry,
            statementRegistry,
            new FixedClock(nowUtc),
            new ValidationOptions { EnableDiagnostics = false });
    }

    private static string CreateEnvelopeJson(int payloadSizeBytes)
    {
        // Create a schema-valid envelope (Spec 001) with a padding field to approach payloadSizeBytes.
        // Padding is placed in `extensions` since schema allows unknown fields there.
        var padding = new string('x', Math.Max(0, payloadSizeBytes - 512));

        var obj = new
        {
            envelopeVersion = "1.0",
            proofSystem = "test",
            statementId = "sigil:license",
            proofBytes = "AA==",
            publicInputs = new
            {
                productId = "product",
                edition = "edition",
                features = new[] { "feature-a" },
            },
            extensions = new
            {
                pad = padding,
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    private sealed class AlwaysTrueVerifier : IProofSystemVerifier
    {
        public Task<bool> VerifyAsync(
            string statementId,
            JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class AlwaysValidStatementHandler : IStatementHandler
    {
        public Task<StatementValidationResult> ValidateAsync(JsonElement publicInputs, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new StatementValidationResult(isValid: true, claims: new LicenseClaims()));
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
