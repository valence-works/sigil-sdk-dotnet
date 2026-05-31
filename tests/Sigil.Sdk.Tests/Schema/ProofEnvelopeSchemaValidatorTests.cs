using System.Text.Json;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Xunit;

namespace Sigil.Sdk.Tests.Schema;

public sealed class ProofEnvelopeSchemaValidatorTests
{
    private readonly ProofEnvelopeSchemaValidator validator = new();

    [Fact]
    public void Validate_ValidLicenseEnvelope_ReturnsValid()
    {
        using var document = JsonDocument.Parse(EnvelopeJson());

        var result = validator.Validate(document.RootElement, diagnosticsEnabled: true);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_UnknownPublicInputField_ReturnsInvalid()
    {
        using var document = JsonDocument.Parse(EnvelopeJson(",\"unexpected\":\"value\""));

        var result = validator.Validate(document.RootElement, diagnosticsEnabled: true);

        Assert.False(result.IsValid);
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public void Validate_DuplicateFeature_ReturnsInvalid()
    {
        using var document = JsonDocument.Parse(EnvelopeJson(features: "[\"feature-a\",\"feature-a\"]"));

        var result = validator.Validate(document.RootElement, diagnosticsEnabled: true);

        Assert.False(result.IsValid);
        Assert.True(result.ErrorCount > 0);
    }

    private static string EnvelopeJson(string publicInputSuffix = "", string features = "[\"feature-a\",\"zk-proof\"]")
    {
        return "{"
            + "\"envelopeVersion\":\"1.0\","
            + $"\"proofSystem\":\"{ProofSystemIds.MidnightZkV1}\","
            + $"\"statementId\":\"{StatementIds.LicenseV1}\","
            + "\"proofBytes\":\"AQ==\","
            + "\"issuedAt\":\"2026-05-31T00:00:00Z\","
            + "\"policyHash\":\"policy-hash\","
            + "\"publicInputs\":{"
            + "\"productId\":\"product-a\","
            + "\"edition\":\"pro\","
            + $"\"features\":{features},"
            + "\"expiresAt\":1893456000,"
            + "\"maxSeats\":5"
            + publicInputSuffix
            + "},"
            + "\"extensions\":{}"
            + "}";
    }
}
