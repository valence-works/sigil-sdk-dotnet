// Spec 002 (FR-007): Immutable DI-constructed statement registry.

using Sigil.Sdk.Statements;

namespace Sigil.Sdk.Registries;

public sealed class ImmutableStatementRegistry : IStatementRegistry
{
    private readonly IReadOnlyDictionary<string, IStatementHandler> handlers;

    public ImmutableStatementRegistry(IEnumerable<KeyValuePair<string, IStatementHandler>> handlers)
    {
        if (handlers is null)
        {
            throw new ArgumentNullException(nameof(handlers));
        }

        var dict = new Dictionary<string, IStatementHandler>(StringComparer.Ordinal);
        foreach (var kvp in handlers)
        {
            if (!dict.TryAdd(kvp.Key, kvp.Value))
            {
                throw new ArgumentException($"Duplicate statement registry key '{kvp.Key}'.", nameof(handlers));
            }
        }

        this.handlers = dict;
    }

    public bool TryGetHandler(string statementId, out IStatementHandler handler)
    {
        return handlers.TryGetValue(statementId, out handler!);
    }
}
