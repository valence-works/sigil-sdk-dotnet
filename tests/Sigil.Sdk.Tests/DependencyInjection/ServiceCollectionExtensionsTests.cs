// Spec 003 (Phase 2 T006-T013): DI integration tests for service registration and validation.

using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.DependencyInjection;

/// <summary>
/// Tests for ServiceCollectionExtensions DI registration methods.
/// Verifies Phase 2 requirements: T006-T013 service registration and validation.
/// Spec 003 (FR-001-FR-017): DI integration and defaults.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region T006 - ISigilValidator Registration

    /// <summary>
    /// T006: Register ISigilValidator as singleton in AddSigilValidation()
    /// Spec 003 (FR-002): The extension method MUST register ISigilValidator
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersISigilValidator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - ISigilValidator should be resolvable
        var validator = provider.GetService<ILicenseValidator>();
        Assert.NotNull(validator);
    }

    /// <summary>
    /// T006: ISigilValidator should be singleton
    /// Verifies the same instance is returned on multiple resolutions
    /// </summary>
    [Fact]
    public void AddSigilValidation_ISigilValidatorIsSingleton()
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

    #endregion

    #region T007 - IProofSystemRegistry Registration

    /// <summary>
    /// T007: Register immutable IProofSystemRegistry as singleton
    /// Spec 003 (FR-003): Registries must use empty default state
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersEmptyProofSystemRegistry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - ProofSystemRegistry should be resolvable and immutable
        var registry = provider.GetService<IProofSystemRegistry>();
        Assert.NotNull(registry);
        Assert.IsType<ImmutableProofSystemRegistry>(registry);
    }

    #endregion

    #region T008 - IStatementRegistry Registration

    /// <summary>
    /// T008: Register immutable IStatementRegistry as singleton
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersEmptyStatementRegistry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert
        var registry = provider.GetService<IStatementRegistry>();
        Assert.NotNull(registry);
        Assert.IsType<ImmutableStatementRegistry>(registry);
    }

    #endregion

    #region T009 - IProofEnvelopeSchemaValidator Registration

    /// <summary>
    /// T009: Register IProofEnvelopeSchemaValidator as singleton (compiled once)
    /// Spec 003 (FR-004): Schema validator must be registered and compiled exactly once
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersSchemaValidator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IProofEnvelopeSchemaValidator>();
        Assert.NotNull(validator);
        Assert.IsType<ProofEnvelopeSchemaValidator>(validator);
    }

    /// <summary>
    /// T009: Schema validator should be singleton (compiled once)
    /// </summary>
    [Fact]
    public void AddSigilValidation_SchemaValidatorIsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Act
        var validator1 = provider.GetService<IProofEnvelopeSchemaValidator>();
        var validator2 = provider.GetService<IProofEnvelopeSchemaValidator>();

        // Assert - Same instance
        Assert.Same(validator1, validator2);
    }

    #endregion

    #region T010 - IClock Registration

    /// <summary>
    /// T010: Register IClock as singleton with SystemClock default
    /// Spec 003 (FR-005): Default clock implementation must be registered
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersClockAsSystemClock()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert
        var clock = provider.GetService<IClock>();
        Assert.NotNull(clock);
        Assert.IsType<SystemClock>(clock);
    }

    #endregion

    #region T011 - ValidationOptions Registration with Defaults

    /// <summary>
    /// T011: Register ValidationOptions as singleton with production-ready defaults
    /// Spec 003 (FR-010): Secure defaults (no sensitive data logging, diagnostics off)
    /// </summary>
    [Fact]
    public void AddSigilValidation_RegistersValidationOptionsWithSecureDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<ValidationOptions>();
        Assert.NotNull(options);
        Assert.False(options.EnableDiagnostics, "EnableDiagnostics should default to false");
        Assert.False(options.LogFailureDetails, "LogFailureDetails should default to false (no sensitive data)");
    }

    #endregion

    #region T012 & T013 - Validation & Null Safety

    /// <summary>
    /// T012: AddSigilValidation throws ArgumentNullException for null services
    /// Spec 003 (FR-016a): Missing or misconfigured dependencies must throw
    /// </summary>
    [Fact]
    public void AddSigilValidation_ThrowsOnNullServices()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddSigilValidation(null!));
    }

    /// <summary>
    /// FR-016a: Misconfigured dependencies should throw InvalidOperationException with guidance
    /// </summary>
    [Fact]
    public void AddSigilValidation_MisconfiguredOptions_WrapsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddSigilValidation(options =>
            {
                options.AddProofSystem(" ", new MockProofSystemVerifier());
            });
        });

        Assert.Contains("error occurred while registering Sigil validation services", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(ex.InnerException);
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    /// <summary>
    /// FR-016b: Exceptions from custom verifiers during registration should be wrapped
    /// </summary>
    [Fact]
    public void AddSigilValidation_CustomVerifierConstructionThrows_WrapsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddSigilValidation(options =>
            {
                options.AddProofSystem("throwing", new ThrowingVerifier());
            });
        });

        Assert.Contains("error occurred while registering Sigil validation services", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(ex.InnerException);
        Assert.Contains("Verifier construction failed", ex.InnerException!.Message);
    }

    /// <summary>
    /// T013: Null safety - optional configureOptions can be null
    /// </summary>
    [Fact]
    public void AddSigilValidation_SucceedsWithNullConfigureDelegate()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - configureOptions is null by default
        var result = services.AddSigilValidation(configureOptions: null);

        // Assert - Should return services for fluent chaining
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    #endregion

    #region FR-011 - Duplicate Call Guard

    /// <summary>
    /// FR-011: AddSigilValidation throws InvalidOperationException on second call
    /// Spec 003 (FR-011): The extension method MUST throw if called multiple times
    /// </summary>
    [Fact]
    public void AddSigilValidation_ThrowsOnDuplicateCall()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddSigilValidation());
        Assert.Contains("already been called", ex.Message);
        Assert.Contains("Spec 003", ex.Message);
    }

    /// <summary>
    /// FR-011: Duplicate call error message should be clear and actionable
    /// </summary>
    [Fact]
    public void AddSigilValidation_DuplicateCallErrorMessageIsActionable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddSigilValidation());

        // Assert - Message should provide guidance
        Assert.Contains("Call it only once", ex.Message);
        Assert.Contains("configureOptions", ex.Message);
    }

    #endregion

    #region Configuration Delegate Tests

    /// <summary>
    /// FR-006: Configuration delegate is invoked and options are applied
    /// </summary>
    [Fact]
    public void AddSigilValidation_ConfigureDelegateIsInvoked()
    {
        // Arrange
        var services = new ServiceCollection();
        var delegateInvoked = false;

        // Act
        services.AddSigilValidation(options =>
        {
            delegateInvoked = true;
            options.EnableDiagnostics = true;
        });

        // Assert
        Assert.True(delegateInvoked);

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<ValidationOptions>();
        Assert.True(options!.EnableDiagnostics);
    }

    /// <summary>
    /// FR-006: Configuration changes are applied to registered options
    /// </summary>
    [Fact]
    public void AddSigilValidation_ConfigurationChangesArePersisted()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation(options =>
        {
            options.EnableDiagnostics = true;
            options.LogFailureDetails = true;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<ValidationOptions>();

        // Assert
        Assert.True(options!.EnableDiagnostics);
        Assert.True(options.LogFailureDetails);
    }

    #endregion

    #region FR-007 & FR-008 - Custom Systems via Configuration

    /// <summary>
    /// FR-007: Custom proof system can be registered via options.AddProofSystem()
    /// </summary>
    [Fact]
    public void AddSigilValidation_CustomProofSystemCanBeRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVerifier = new MockProofSystemVerifier();

        // Act
        services.AddSigilValidation(options =>
        {
            options.AddProofSystem("test-system", mockVerifier);
        });

        var provider = services.BuildServiceProvider();
        var registry = provider.GetService<IProofSystemRegistry>();

        // Assert
        Assert.True(registry!.TryGetVerifier("test-system", out var verifier));
        Assert.Same(mockVerifier, verifier);
    }

    /// <summary>
    /// FR-007: Duplicate proof system identifier throws InvalidOperationException
    /// </summary>
    [Fact]
    public void AddSigilValidation_DuplicateProofSystemThrows()
    {
        // Arrange
        var services = new ServiceCollection();
        var verifier1 = new MockProofSystemVerifier();
        var verifier2 = new MockProofSystemVerifier();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddSigilValidation(options =>
            {
                options.AddProofSystem("duplicate-id", verifier1);
                options.AddProofSystem("duplicate-id", verifier2);
            });
        });

        Assert.Contains("already registered", ex.Message);
        Assert.Contains("Spec 003", ex.Message);
    }

    /// <summary>
    /// FR-008: Custom statement handler can be registered via options.AddStatementHandler()
    /// </summary>
    [Fact]
    public void AddSigilValidation_CustomStatementHandlerCanBeRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHandler = new MockStatementHandler();

        // Act
        services.AddSigilValidation(options =>
        {
            options.AddStatementHandler(mockHandler);
        });

        var provider = services.BuildServiceProvider();
        var registry = provider.GetService<IStatementRegistry>();

        // Assert
        Assert.NotNull(registry);
        // Handler should be in the registry (keyed by StatementId)
        Assert.True(registry.TryGetHandler(mockHandler.StatementId, out var handler));
    }

    /// <summary>
    /// FR-008: Duplicate statement handler ID throws InvalidOperationException
    /// </summary>
    [Fact]
    public void AddSigilValidation_DuplicateStatementHandlerTypeThrows()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler1 = new MockStatementHandler();
        var handler2 = new MockStatementHandler();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddSigilValidation(options =>
            {
                options.AddStatementHandler(handler1);
                options.AddStatementHandler(handler2);
            });
        });

        Assert.Contains("already registered", ex.Message);
        Assert.Contains("Spec 004", ex.Message);
    }

    #endregion

    #region FR-006 - Fluent Builder Chaining

    /// <summary>
    /// FR-006: Configuration methods support fluent chaining
    /// </summary>
    [Fact]
    public void ValidationOptions_SupportsFuentChaining()
    {
        // Arrange
        var verifier = new MockProofSystemVerifier();
        var handler = new MockStatementHandler();

        // Act - Fluent chaining should work
        var options = new ValidationOptions()
            .AddProofSystem("system1", verifier)
            .AddStatementHandler(handler);

        // Assert
        options.AddProofSystem("system2", verifier); // Should be able to continue chaining
    }

    #endregion

    #region Backward Compatibility - Legacy Overload

    /// <summary>
    /// Backward compatibility: Old overload with IEnumerable still works
    /// </summary>
    [Fact]
    public void AddSigilValidation_LegacyOverloadStillWorks()
    {
        // Arrange
        var services = new ServiceCollection();
        var proofSystems = new Dictionary<string, IProofSystemVerifier>
        {
            { "legacy-system", new MockProofSystemVerifier() }
        };
        var handlers = new Dictionary<string, IStatementHandler>
        {
            { "legacy-handler", new MockStatementHandler("legacy-handler") }
        };

        // Act - Use legacy overload
        services.AddSigilValidation(proofSystems, handlers);
        var provider = services.BuildServiceProvider();

        // Assert
        var registry = provider.GetService<IProofSystemRegistry>();
        Assert.True(registry!.TryGetVerifier("legacy-system", out _));
    }

    #endregion

    #region Integration - All Services Wired Together

    /// <summary>
    /// Integration test: All services are properly wired and can be resolved
    /// </summary>
    [Fact]
    public void AddSigilValidation_AllServicesDependenciesAreWired()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - All services should be resolvable
        var validator = provider.GetService<ILicenseValidator>();
        var schemaValidator = provider.GetService<IProofEnvelopeSchemaValidator>();
        var proofRegistry = provider.GetService<IProofSystemRegistry>();
        var statementRegistry = provider.GetService<IStatementRegistry>();
        var clock = provider.GetService<IClock>();
        var options = provider.GetService<ValidationOptions>();

        Assert.NotNull(validator);
        Assert.NotNull(schemaValidator);
        Assert.NotNull(proofRegistry);
        Assert.NotNull(statementRegistry);
        Assert.NotNull(clock);
        Assert.NotNull(options);
    }

    #endregion

    #region T027-T029 - Diagnostics Configuration

    /// <summary>
    /// Spec 003 (T024): LogFailureDetails can be configured during DI registration
    /// </summary>
    [Fact]
    public void AddSigilValidation_CanConfigureLogFailureDetails()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure LogFailureDetails to true
        services.AddSigilValidation(options => options.LogFailureDetails = true);
        var provider = services.BuildServiceProvider();

        // Assert - Options should have LogFailureDetails set
        var registeredOptions = provider.GetService<ValidationOptions>();
        Assert.NotNull(registeredOptions);
        Assert.True(registeredOptions.LogFailureDetails);
    }

    /// <summary>
    /// Spec 003 (T024): LogFailureDetails defaults to false (secure default)
    /// </summary>
    [Fact]
    public void AddSigilValidation_LogFailureDetailsDefaultsToFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - Secure default is false
        var options = provider.GetService<ValidationOptions>();
        Assert.NotNull(options);
        Assert.False(options.LogFailureDetails);
    }

    /// <summary>
    /// Spec 003 (T025): EnableDiagnostics can be configured during DI registration
    /// </summary>
    [Fact]
    public void AddSigilValidation_CanConfigureEnableDiagnostics()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure EnableDiagnostics to true
        services.AddSigilValidation(options => options.EnableDiagnostics = true);
        var provider = services.BuildServiceProvider();

        // Assert - Options should have EnableDiagnostics set
        var registeredOptions = provider.GetService<ValidationOptions>();
        Assert.NotNull(registeredOptions);
        Assert.True(registeredOptions.EnableDiagnostics);
    }

    /// <summary>
    /// Spec 003 (T025): EnableDiagnostics defaults to false (secure default)
    /// </summary>
    [Fact]
    public void AddSigilValidation_EnableDiagnosticsDefaultsToFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert - Secure default is false
        var options = provider.GetService<ValidationOptions>();
        Assert.NotNull(options);
        Assert.False(options.EnableDiagnostics);
    }

    /// <summary>
    /// Spec 003 (T026): Multiple diagnostics options can be configured together
    /// </summary>
    [Fact]
    public void AddSigilValidation_CanConfigureMultipleDiagnosticsOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure both options
        services.AddSigilValidation(options =>
        {
            options.EnableDiagnostics = true;
            options.LogFailureDetails = true;
        });
        var provider = services.BuildServiceProvider();

        // Assert - Both options set correctly
        var registeredOptions = provider.GetService<ValidationOptions>();
        Assert.NotNull(registeredOptions);
        Assert.True(registeredOptions.EnableDiagnostics);
        Assert.True(registeredOptions.LogFailureDetails);
    }

    /// <summary>
    /// Spec 003 (T027): Diagnostics configuration doesn't affect core validation
    /// Verifies that enabling diagnostics doesn't prevent validation from working
    /// </summary>
    [Fact]
    public void AddSigilValidation_DiagnosticsConfigurationDoesntAffectValidation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configure diagnostics
        services.AddSigilValidation(options => options.EnableDiagnostics = true);
        var provider = services.BuildServiceProvider();

        // Assert - Validator still resolvable and is singleton
        var validator1 = provider.GetService<ILicenseValidator>();
        var validator2 = provider.GetService<ILicenseValidator>();
        
        Assert.NotNull(validator1);
        Assert.NotNull(validator2);
        Assert.Same(validator1, validator2);
    }

    #endregion

    #region T047 - Registry Immutability Tests

    /// <summary>
    /// T047: Verify runtime modification of ImmutableProofSystemRegistry throws InvalidOperationException
    /// Spec 003 (FR-015): Registries are immutable after container build
    /// </summary>
    [Fact]
    public void ImmutableProofSystemRegistry_AddVerifier_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();
        var registry = (ImmutableProofSystemRegistry)provider.GetRequiredService<IProofSystemRegistry>();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            registry.AddVerifier("new-system", new MockProofSystemVerifier()));
        
        Assert.Contains("immutable after container build", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Spec 003 FR-015", ex.Message);
        Assert.Contains("ValidationOptions", ex.Message);
    }

    /// <summary>
    /// T047: Verify runtime modification of ImmutableStatementRegistry throws InvalidOperationException
    /// Spec 003 (FR-015): Registries are immutable after container build
    /// </summary>
    [Fact]
    public void ImmutableStatementRegistry_AddHandler_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();
        var registry = (ImmutableStatementRegistry)provider.GetRequiredService<IStatementRegistry>();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            registry.AddHandler("new-statement", new MockStatementHandler()));
        
        Assert.Contains("immutable after container build", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Spec 003 FR-015", ex.Message);
        Assert.Contains("ValidationOptions", ex.Message);
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Mock IProofSystemVerifier for testing
    /// </summary>
    private class MockProofSystemVerifier : IProofSystemVerifier
    {
        public Task<bool> VerifyAsync(
            string statementId,
            System.Text.Json.JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Mock IStatementHandler for testing
    /// </summary>
    private class MockStatementHandler : IStatementHandler
    {
        public MockStatementHandler(string? statementId = null)
        {
            StatementId = string.IsNullOrWhiteSpace(statementId)
                ? "urn:example:statement:mock"
                : statementId;
        }

        public string StatementId { get; }

        public Task<StatementValidationResult> ValidateAsync(
            System.Text.Json.JsonElement publicInputs,
            CancellationToken cancellationToken = default)
        {
            var claims = new LicenseClaims(
                productId: "product",
                edition: "edition",
                features: new[] { "feature-a" },
                expiresAt: 1,
                maxSeats: 1,
                issuedAt: 1);
            return Task.FromResult(new StatementValidationResult(true, claims));
        }
    }

    private sealed class ThrowingVerifier : IProofSystemVerifier
    {
        public ThrowingVerifier()
        {
            throw new InvalidOperationException("Verifier construction failed.");
        }

        public Task<bool> VerifyAsync(
            string statementId,
            System.Text.Json.JsonElement publicInputs,
            ReadOnlyMemory<byte> proofBytes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    #endregion
}
