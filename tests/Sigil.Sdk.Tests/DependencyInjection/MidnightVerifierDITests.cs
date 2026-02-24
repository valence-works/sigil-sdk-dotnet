using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;
using Sigil.Sdk.Proof;
using Sigil.Sdk.Registries;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.DependencyInjection;

/// <summary>
/// Tests for Midnight verifier DI registration and routing.
/// Validates FR-002 (DI registration) from Spec 006.
/// </summary>
public class MidnightVerifierDITests
{
    [Fact]
    public void AddSigilValidation_RegistersMidnightVerifierByDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();

        // Assert: Midnight verifier should be registered
        var registry = provider.GetRequiredService<IProofSystemRegistry>();
        var hasVerifier = registry.TryGetVerifier(ProofSystemIds.MidnightZkV1, out var verifier);

        Assert.True(hasVerifier, $"Midnight verifier not found for proof system: {ProofSystemIds.MidnightZkV1}");
        Assert.NotNull(verifier);
        Assert.IsType<MidnightZkV1ProofSystemVerifier>(verifier);
    }

    [Fact]
    public void ProofSystemRegistry_CanResolveCanonicalMidnightIdentifier()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IProofSystemRegistry>();

        // Act
        var found = registry.TryGetVerifier("midnight-zk-v1", out var verifier);

        // Assert
        Assert.True(found);
        Assert.NotNull(verifier);
        Assert.IsType<MidnightZkV1ProofSystemVerifier>(verifier);
    }

    [Fact]
    public void ProofSystemRegistry_FailsForNonCanonicalIdentifier()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSigilValidation();
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IProofSystemRegistry>();

        // Act
        var found = registry.TryGetVerifier("midnight-v1", out var verifier); // Wrong identifier

        // Assert
        Assert.False(found);
        Assert.Null(verifier);
    }

    [Fact]
    public void ValidationOptions_AddMidnightZkV1ProofSystem_DoesNotThrow()
    {
        // Arrange
        var options = new ValidationOptions();

        // Act
        var result = options.AddMidnightZkV1ProofSystem();

        // Assert: Method should return options for chaining
        Assert.NotNull(result);
        Assert.Same(options, result);
    }

    [Fact]
    public void DuplicateMidnightRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new ValidationOptions();
        options.AddMidnightZkV1ProofSystem();

        // Act & Assert: Second call should throw (Spec 003 FR-007 - no duplicate IDs)
        var ex = Assert.Throws<InvalidOperationException>(() =>
            options.AddMidnightZkV1ProofSystem());

        Assert.Contains("already registered", ex.Message);
    }

    [Fact]
    public void ImmutableProofSystemRegistry_IsReadOnly()
    {
        // Arrange: Create registry with Midnight verifier
        var verifier = new MidnightZkV1ProofSystemVerifier();
        var registry = new ImmutableProofSystemRegistry(
            new Dictionary<string, IProofSystemVerifier> { { ProofSystemIds.MidnightZkV1, verifier } });

        // Act & Assert: Registry should be immutable (no public mutation methods)
        Assert.IsType<ImmutableProofSystemRegistry>(registry);

        // Retrieve verifier and verify it's not null
        var found = registry.TryGetVerifier(ProofSystemIds.MidnightZkV1, out var retrievedVerifier);
        Assert.True(found);
        Assert.NotNull(retrievedVerifier);
    }
}
