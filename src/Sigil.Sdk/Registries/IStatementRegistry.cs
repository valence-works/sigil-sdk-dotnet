// Spec 002 (FR-007): Immutable registry contract for statement handlers.

using Sigil.Sdk.Statements;

namespace Sigil.Sdk.Registries;

public interface IStatementRegistry
{
    bool TryGetHandler(string statementId, out IStatementHandler handler);
}
