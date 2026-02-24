// Spec 003 (FR-001-FR-017): DI extension method with registration and configuration.
// Spec 002 (FR-009): Ensure schema validator is initialized once via DI.

using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.DependencyInjection;

/// <summary>
/// ServiceCollection extension methods for Sigil validator dependency injection registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Unique marker type for detecting if AddSigilValidation has been called.
    /// Used to implement the duplicate-call guard (Spec 003 FR-011).
    /// </summary>
    private sealed class SigilValidationMarker
    {
        public static readonly SigilValidationMarker Instance = new();
    }

    /// <summary>
    /// Registers the Sigil validator with dependency injection using default configuration.
    /// This is the simplest way to integrate Sigil validation into a .NET application.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services in</param>
    /// <returns>The IServiceCollection for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">If services is null</exception>
    /// <exception cref="InvalidOperationException">If AddSigilValidation has already been called (Spec 003 FR-011)</exception>
    /// <remarks>
    /// <para>
    /// This method registers:
    /// - ISigilValidator (singleton): The main validator service
    /// - IProofSystemRegistry (singleton): Immutable registry of proof systems (empty by default)
    /// - IStatementRegistry (singleton): Immutable registry of statement handlers (empty by default)
    /// - IProofEnvelopeSchemaValidator (singleton): Compiled schema validator
    /// - IClock (singleton): System clock for time-based validation
    /// - ValidationOptions (singleton): Configuration with secure defaults
    /// </para>
    /// <para>
    /// All registrations use singleton lifetime for performance and consistency.
    /// Schema validation compilation happens exactly once at startup.
    /// </para>
    /// <para>
    /// For minimal setup, this 1-line call is sufficient:
    /// <code>
    /// services.AddSigilValidation();
    /// </code>
    /// </para>
    /// <para>
    /// For customized setup with proof systems or diagnostics, use the overload with a configuration delegate:
    /// <code>
    /// services.AddSigilValidation(options =>
    /// {
    ///     options.AddProofSystem("paillier", new MyPaillierVerifier());
    ///     options.LogFailureDetails = true;
    /// });
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddSigilValidation(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        return AddSigilValidation(services, configureOptions: null);
    }

    /// <summary>
    /// Registers the Sigil validator with dependency injection using custom configuration.
    /// This method enables customization of proof systems, statement handlers, and diagnostics options.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services in</param>
    /// <param name="configureOptions">Optional delegate to configure ValidationOptions with custom systems and handlers</param>
    /// <returns>The IServiceCollection for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">If services is null</exception>
    /// <exception cref="InvalidOperationException">
    /// If AddSigilValidation has already been called (Spec 003 FR-011), or if
    /// a custom proof system verifier throws during registration (Spec 003 FR-016b)
    /// </exception>
    /// <remarks>
    /// <para>
    /// The configureOptions delegate is invoked immediately to populate custom systems and handlers
    /// into the ValidationOptions before registries are constructed.
    /// Any exceptions thrown during this delegate (including duplicate identifier errors from
    /// builder methods) will propagate immediately to provide fail-fast behavior.
    /// </para>
    /// <para>
    /// Example with custom proof system and diagnostics:
    /// <code>
    /// services.AddSigilValidation(options =>
    /// {
    ///     options.AddProofSystem("paillier", new PaillierVerifier())
    ///            .AddProofSystem("groth16", new Groth16Verifier());
    ///     options.AddStatementHandler(new MyStatementHandler());
    ///     options.EnableDiagnostics = true;
    ///     options.LogFailureDetails = false; // No sensitive data in logs
    /// });
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddSigilValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configureOptions = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Spec 003 (FR-011): Throw if AddSigilValidation called multiple times
        ThrowIfAlreadyRegistered(services);

        try
        {
            // Spec 003 (FR-011): Check for duplicate call first
            ThrowIfAlreadyRegistered(services);

            // Register schema validator first (compiled once, used by validator)
            services.AddSingleton<IProofEnvelopeSchemaValidator, ProofEnvelopeSchemaValidator>();

            // Create and configure options
            var options = new ValidationOptions();
            options.AddMidnightZkV1ProofSystem();
            options.AddStatementHandler(new LicenseV1StatementHandler());
            configureOptions?.Invoke(options);

            // Spec 003 (FR-010): Production-ready defaults
            // LogFailureDetails defaults to false (no sensitive data logging)
            // EnableDiagnostics defaults to false

            // Register options as singleton
            services.AddSingleton(options);

            // Spec 003 (FR-003, FR-004): Register immutable registries with custom systems from options
            services.AddSingleton<IProofSystemRegistry>(_ =>
                new ImmutableProofSystemRegistry(options.ProofSystems));

            services.AddSingleton<IStatementRegistry>(_ =>
                new ImmutableStatementRegistry(options.StatementHandlers));

            // Register clock abstraction
            services.AddSingleton<IClock, SystemClock>();

            // Register validator (depends on schema validator, registries, options, clock)
            services.AddSingleton<ILicenseValidator, LicenseValidator>();

            // Mark that AddSigilValidation has been successfully registered (must be last)
            services.AddSingleton(SigilValidationMarker.Instance);

            return services;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException && ex.Message.Contains("already registered")))
        {
            // Wrap non-InvalidOperationException errors in InvalidOperationException for consistent failure mode
            // Spec 003 (FR-016b): Exceptions from custom verifiers should be wrapped
            throw new InvalidOperationException(
                "An error occurred while registering Sigil validation services. Please check the inner exception for details.",
                ex);
        }
    }

    /// <summary>
    /// Legacy overload for backward compatibility with code providing proof systems and statements directly.
    /// This overload is maintained for API compatibility but the parameterless or
    /// configureOptions overloads are preferred for cleaner setup.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services in</param>
    /// <param name="proofSystems">Pre-configured collection of proof system verifiers</param>
    /// <param name="statements">Pre-configured collection of statement handlers</param>
    /// <param name="configureOptions">Optional delegate for additional configuration</param>
    /// <returns>The IServiceCollection for fluent chaining</returns>
    [Obsolete("Use AddSigilValidation() or AddSigilValidation(Action<ValidationOptions>) instead. This overload will be removed in a future version.", false)]
    public static IServiceCollection AddSigilValidation(
        this IServiceCollection services,
        IEnumerable<KeyValuePair<string, IProofSystemVerifier>> proofSystems,
        IEnumerable<KeyValuePair<string, IStatementHandler>> statements,
        Action<ValidationOptions>? configureOptions = null)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (proofSystems is null)
        {
            throw new ArgumentNullException(nameof(proofSystems));
        }

        if (statements is null)
        {
            throw new ArgumentNullException(nameof(statements));
        }

        // Delegate to main method by converting collections to builder calls
        return AddSigilValidation(services, options =>
        {
            foreach (var kvp in proofSystems)
            {
                options.AddProofSystem(kvp.Key, kvp.Value);
            }

            foreach (var kvp in statements)
            {
                options.AddStatementHandler(kvp.Value);
            }

            configureOptions?.Invoke(options);
        });
    }

    /// <summary>
    /// Registers schema validation when used standalone (without full Sigilvalidator setup).
    /// This is primarily used internally by AddSigilValidation but can be called directly
    /// if only schema validation is needed.
    /// </summary>
    /// <param name="services">The IServiceCollection to register services in</param>
    /// <returns>The IServiceCollection for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">If services is null</exception>
    /// <remarks>
    /// This method is automatically called by AddSigilValidation and should generally
    /// not be called directly unless you're doing advanced configuration.
    /// Spec 003 (FR-004): Schema validator is compiled exactly once via singleton registration.
    /// </remarks>
    public static IServiceCollection AddSigilProofEnvelopeSchemaValidation(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Spec 002 (FR-009): Singleton schema compilation/initialization for optimal startup performance.
        services.AddSingleton<IProofEnvelopeSchemaValidator, ProofEnvelopeSchemaValidator>();
        return services;
    }

    /// <summary>
    /// Checks if AddSigilValidation has already been registered to prevent duplicate registrations.
    /// Spec 003 (FR-011): Throw InvalidOperationException on second call.
    /// </summary>
    private static void ThrowIfAlreadyRegistered(IServiceCollection services)
    {
        // Check if the marker has already been registered
        if (services.Any(sd => sd.ServiceType == typeof(SigilValidationMarker)))
        {
            throw new InvalidOperationException(
                "AddSigilValidation has already been called on this service collection. " +
                "Call it only once during application startup. " +
                "To reconfigure validation options, use the configureOptions parameter in a single call instead. " +
                "(Spec 003 FR-011)");
        }
    }
}
