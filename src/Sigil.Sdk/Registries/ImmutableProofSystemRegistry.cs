// Spec 002 (FR-007): Immutable DI-constructed proof system registry.

using Sigil.Sdk.Proof;

namespace Sigil.Sdk.Registries;

public sealed class ImmutableProofSystemRegistry : IProofSystemRegistry
{
    private readonly IReadOnlyDictionary<string, IProofSystemVerifier> verifiers;

    public ImmutableProofSystemRegistry(IEnumerable<KeyValuePair<string, IProofSystemVerifier>> verifiers)
    {
        if (verifiers is null)
        {
            throw new ArgumentNullException(nameof(verifiers));
        }

        var dict = new Dictionary<string, IProofSystemVerifier>(StringComparer.Ordinal);
        foreach (var kvp in verifiers)
        {
            if (!dict.TryAdd(kvp.Key, kvp.Value))
            {
                throw new ArgumentException($"Duplicate proofSystem registry key '{kvp.Key}'.", nameof(verifiers));
            }
        }

        this.verifiers = dict;
    }

    public bool TryGetVerifier(string proofSystem, out IProofSystemVerifier verifier)
    {
        return verifiers.TryGetValue(proofSystem, out verifier!);
    }

    /// <summary>
    /// Spec 003 (FR-015): Registries are immutable after container build.
    /// This method exists only to provide a clear failure mode if mutation is attempted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown to enforce immutability.</exception>
    public void AddVerifier(string proofSystem, IProofSystemVerifier verifier)
    {
        throw new InvalidOperationException(
            "Registries are immutable after container build (Spec 003 FR-015). Use ValidationOptions during registration.");
    }
}
