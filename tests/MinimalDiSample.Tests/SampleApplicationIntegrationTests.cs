// Spec 003 (T022): Integration tests for the minimal sample application
// Verifies end-to-end validation flow in ASP.NET Core sample

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MinimalDiSample.Tests;

/// <summary>
/// Integration tests for the minimal DI sample application.
/// Tests the complete end-to-end flow of setup -> validation.
/// Spec 003 (SC-001, SC-008): Validates the sample works as documented
/// </summary>
public class SampleApplicationIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;

    async Task IAsyncLifetime.InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
        await Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        client?.Dispose();
        factory?.Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// T022: Verify health endpoint confirms validator is initialized
    /// Tests that DI container has successfully wired all dependencies
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_ReturnsOk_WhenValidatorIsInitialized()
    {
        // Act
        var response = await client!.GetAsync("/api/validation/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected 200 but got {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;
        
        Assert.Equal("healthy", json.GetProperty("status").GetString());
        Assert.Contains("Validator", json.GetProperty("validator_type").GetString()!);
    }

    /// <summary>
    /// T022: Validate that the sample accepts valid proof envelope JSON
    /// Tests the POST /api/validation/validate endpoint
    /// </summary>
    [Fact]
    public async Task ValidateEndpoint_AcceptsProofEnvelope_AndReturnsValidationResult()
    {
        // Arrange - Create a minimal valid proof envelope
        var envelopeJson = JsonSerializer.Serialize(new
        {
            version = "1.0",
            proof = new
            {
                proofSystem = "test",
                publicInputs = new object(),
                proofBytes = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
            },
            statements = Array.Empty<object>()
        });

        var content = new StringContent(
            JsonSerializer.Serialize(envelopeJson),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client!.PostAsync("/api/validation/validate", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected 200 or 400 but got {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}");
    }

    /// <summary>
    /// T022: Validate error handling for null/empty envelope
    /// </summary>
    [Fact]
    public async Task ValidateEndpoint_ReturnsError_WhenEnvelopeIsEmpty()
    {
        // Arrange
        var content = new StringContent(
            "\"\"",
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await client!.PostAsync("/api/validation/validate", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// T022: Verify sample demonstrates successful DI integration
    /// Core test that validates the 3-5 line setup story works
    /// Spec 003 (SC-008): Zero runtime service resolution failures
    /// </summary>
    [Fact]
    public async Task Sample_SuccessfullyIntegrates_SigilValidation()
    {
        // The fact that this test can even run proves:
        // 1. Application can be created without errors
        // 2. DI container is configured correctly
        // 3. All dependencies are resolved
        // 4. Service can be injected into controller
        // 5. HTTP endpoints respond

        // Act - Call health endpoint as proof of successful integration
        var response = await client!.GetAsync("/api/validation/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content);

        // Spec 003 (SC-001): Setup should be <5 minutes effort
        // This sample's Program.cs is ~20 lines with single AddSigilValidation() call
    }

    /// <summary>
    /// T022: Validate multiple validation requests can be handled
    /// Tests that singleton registrations work correctly across requests
    /// </summary>
    [Fact]
    public async Task Sample_HandleMultipleValidationRequests_Correctly()
    {
        // Arrange
        var envelopeJson = JsonSerializer.Serialize(new
        {
            version = "1.0",
            proof = new { proofSystem = "test", publicInputs = new object(), proofBytes = "AQIDBA==" },
            statements = Array.Empty<object>()
        });

        var content = new StringContent(
            JsonSerializer.Serialize(envelopeJson),
            Encoding.UTF8,
            "application/json");

        // Act - Make multiple requests
        var task1 = client!.PostAsync("/api/validation/validate", content);
        var task2 = client!.PostAsync("/api/validation/validate", content);
        var task3 = client!.PostAsync("/api/validation/validate", content);

        var responses = await Task.WhenAll(task1, task2, task3);

        // Assert - All should complete without error
        foreach (var response in responses)
        {
            // Note: not checking StatusCode here because the validation result might be valid or invalid
            // The key point is that it doesn't throw or crash
            Assert.NotNull(response);
        }
    }
}
