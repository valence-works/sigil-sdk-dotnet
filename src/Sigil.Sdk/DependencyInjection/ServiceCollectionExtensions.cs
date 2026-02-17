// Spec 002 (FR-009): Ensure schema validator is initialized once via DI.

using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Schema;
using Sigil.Sdk.Statements;
using Sigil.Sdk.Time;
using Sigil.Sdk.Validation;

namespace Sigil.Sdk.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSigilProofEnvelopeSchemaValidation(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Spec 002 (FR-009): singleton schema compilation/initialization.
        services.AddSingleton<IProofEnvelopeSchemaValidator, ProofEnvelopeSchemaValidator>();
        return services;
    }

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

        services.AddSigilProofEnvelopeSchemaValidation();
        services.AddSingleton<IClock, SystemClock>();

        var options = new ValidationOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Spec 002 (FR-007): registries are immutable after construction.
        services.AddSingleton<IProofSystemRegistry>(_ => new ImmutableProofSystemRegistry(proofSystems));
        services.AddSingleton<IStatementRegistry>(_ => new ImmutableStatementRegistry(statements));

        services.AddSingleton<ILicenseValidator, LicenseValidator>();
        return services;
    }
}
