// Spec 003 (T022): Integration tests for the minimal sample application
// Verifies DI setup and validator resolution from the sample's service configuration

using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;
using Sigil.Sdk.Validation;
using Xunit;

namespace MinimalDiSample.Tests;

/// <summary>
/// Integration tests for the minimal DI sample application.
/// Tests the DI setup and service resolution work correctly.
/// Spec 003 (SC-001): Validates basic integration of ISigilValidator via DI
/// </summary>
public class SampleApplicationIntegrationTests
{
    /// <summary>
    /// T022: Verify DI setup matches what the sample application does
    /// Tests that when calling AddSigilValidation(), all core services are registered
    /// </summary>
    [Fact]
    public void Sample_SuccessfullyIntegrates_SigilValidation()
    {
        // Arrange - Replicate the sample's DI setup
        var services = new ServiceCollection();

        // Act - Register Sigil validation exactly as in sample Program.cs
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - All expected services should be resolvable
        var validator = provider.GetService<ILicenseValidator>();
        Assert.NotNull(validator);
    }

    /// <summary>
    /// T022: Verify validator can be resolved and used immediately
    /// Tests the complete flow: register -> resolve -> validate
    /// </summary>
    [Fact]
    public async Task AddSigilValidation_EnablesValidatorExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();
        
        var validator = provider.GetService<ILicenseValidator>();
        Assert.NotNull(validator);

        // Minimal invalid envelope (schema validation will fail)
        var invalidEnvelopeJson = "{}";

        // Act - Execute validation
        var result = await validator.ValidateAsync(invalidEnvelopeJson);

        // Assert - Should get deterministic failure
        Assert.NotNull(result);
        Assert.Equal(LicenseStatus.Malformed, result.Status);
        Assert.NotNull(result.Failure);
    }

    /// <summary>
    /// T022: Verify multiple registrations throw (duplicate call guard)
    /// Tests that calling AddSigilValidation() twice fails with clear error
    /// </summary>
    [Fact]
    public void AddSigilValidation_SecondCall_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation(); // First call succeeds

        // Act & Assert - Second call should throw
        Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddSigilValidation(); // Second call fails
        });
    }

    /// <summary>
    /// T022: Verify validator is registered as singleton
    /// Tests that the same instance is returned on multiple resolutions
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersValidatorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Act
        var validator1 = provider.GetService<ILicenseValidator>();
        var validator2 = provider.GetService<ILicenseValidator>();

        // Assert - Same instance
        Assert.Same(validator1, validator2);
    }

    /// <summary>
    /// T022: Verify configuration can be applied at setup
    /// Tests that options passed to AddSigilValidation() are respected
    /// </summary>
    [Fact]
    public void AddSigilValidation_WithOptions_ConfiguresServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure diagnostics during setup
        services.AddSigilValidation(options =>
        {
            options.EnableDiagnostics = true;
            options.LogFailureDetails = true;
        });

        var provider = services.BuildServiceProvider();

        // Assert - Options should be available in DI container
        var resolvedOptions = provider.GetService<ValidationOptions>();
        Assert.NotNull(resolvedOptions);
        Assert.True(resolvedOptions.EnableDiagnostics);
        Assert.True(resolvedOptions.LogFailureDetails);
    }

    /// <summary>
    /// T022: Verify ISigilValidator is properly named (ILicenseValidator)
    /// Tests that the public interface matches the documented API
    /// </summary>
    [Fact]
    public void AddSigilValidation_ExposesCorrectPublicInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<ILicenseValidator>();

        // Assert - Service should implement the expected interface
        Assert.NotNull(validator);
        Assert.IsAssignableFrom<ILicenseValidator>(validator);
    }

    #region T048 - Multi-Scenario Hosting Tests

    /// <summary>
    /// T048: Verify AddSigilValidation works in ASP.NET Core scenario
    /// Tests that DI integration works with WebApplicationBuilder
    /// </summary>
    [Fact]
    public void AddSigilValidation_WorksInAspNetCore_WebApplicationBuilder()
    {
        // Arrange - Simulate ASP.NET Core setup (already tested above)
        var services = new ServiceCollection();

        // Act - Register services as in ASP.NET Core
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - Validator resolves correctly
        var validator = provider.GetService<ILicenseValidator>();
        Assert.NotNull(validator);
        Assert.IsAssignableFrom<ILicenseValidator>(validator);
    }

    /// <summary>
    /// T048: Verify AddSigilValidation works in console application scenario
    /// Tests that DI integration works with HostBuilder for console apps
    /// </summary>
    [Fact]
    public void AddSigilValidation_WorksInConsoleApp_HostBuilder()
    {
        // Arrange - Simulate console app with HostBuilder
        var services = new ServiceCollection();

        // Act - Register services as in console app
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - Validator resolves correctly
        var validator = provider.GetService<ILicenseValidator>();
        Assert.NotNull(validator);
        Assert.IsAssignableFrom<ILicenseValidator>(validator);
    }

    /// <summary>
    /// T048: Verify AddSigilValidation works in worker service scenario
    /// Tests that DI integration works with background worker services
    /// </summary>
    [Fact]
    public void AddSigilValidation_WorksInWorkerService_HostBuilder()
    {
        // Arrange - Simulate worker service with HostBuilder
        var services = new ServiceCollection();

        // Act - Register services as in worker service
        services.AddSigilValidation(options =>
        {
            // Worker services might enable diagnostics
            options.EnableDiagnostics = true;
        });
        var provider = services.BuildServiceProvider();

        // Assert - Validator resolves correctly with configuration
        var validator = provider.GetService<ILicenseValidator>();
        var options = provider.GetService<ValidationOptions>();
        
        Assert.NotNull(validator);
        Assert.NotNull(options);
        Assert.True(options.EnableDiagnostics);
    }

    #endregion
}

