// Spec 002 (FR-007): Immutable registry contract for statement handlers.

using Sigil.Sdk.Statements;

namespace Sigil.Sdk.Registries;

public interface IStatementRegistry
{
    /// <summary>
    /// Attempts to resolve a statement handler by canonical statement identifier (URN).
    /// </summary>
    /// <param name="statementId">Canonical statement identifier.</param>
    /// <param name="handler">Resolved handler when found.</param>
    /// <returns>True when a handler is registered for the statement ID.</returns>
    bool TryGetHandler(string statementId, out IStatementHandler handler);
}
