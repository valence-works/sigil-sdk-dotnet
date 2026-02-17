// Spec 003 (FR-006, FR-007, FR-008): Validation options builder for DI configuration.
// Spec 002 (FR-011): Diagnostics opt-in.

using Sigil.Sdk.Proof;
using Sigil.Sdk.Statements;

namespace Sigil.Sdk.Validation;

/// <summary>
/// Configuration options for Sigil validation setup via dependency injection.
/// Supports builder pattern for adding custom proof systems and statement handlers.
/// </summary>
/// <remarks>
/// <para>
/// Create an instance and use builder methods to customize validation behavior:
/// <code>
/// var options = new ValidationOptions()
///     .AddProofSystem("paillier", new MyPaillierVerifier())
///     .AddStatementHandler(new MyStatementHandler());
/// </code>
/// </para>
/// <para>
/// After service container is built, the registries become immutable.
/// Attempting to modify them will throw InvalidOperationException (Spec 003 FR-015).
/// </para>
/// </remarks>
public sealed class ValidationOptions
{
    private readonly Dictionary<string, IProofSystemVerifier> proofSystems = new(StringComparer.Ordinal);
    private readonly List<IStatementHandler> statementHandlers = new();

    /// <summary>
    /// Gets or sets whether diagnostics are enabled during validation.
    /// Default: false (Spec 003 FR-010 - secure defaults).
    /// </summary>
    public bool EnableDiagnostics { get; set; } = false;

    /// <summary>
    /// Gets or sets whether detailed failure information should be logged.
    /// When false, sensitive details are excluded from logs (Spec 003 FR-016).
    /// Default: false (Spec 003 FR-010 - no sensitive data logging).
    /// </summary>
    public bool LogFailureDetails { get; set; } = false;

    /// <summary>
    /// Gets an enumerable of registered custom proof system verifiers (identifier, verifier) pairs.
    /// </summary>
    internal IEnumerable<KeyValuePair<string, IProofSystemVerifier>> ProofSystems => proofSystems;

    /// <summary>
    /// Gets an enumerable of registered custom statement handlers.
    /// </summary>
    internal IEnumerable<IStatementHandler> StatementHandlers => statementHandlers;

    /// <summary>
    /// Registers a custom proof system verifier with the specified identifier.
    /// </summary>
    /// <param name="identifier">Unique identifier for the proof system (e.g., "paillier", "groth16")</param>
    /// <param name="verifier">Implementation of IProofSystemVerifier for this system</param>
    /// <returns>This instance for builder chaining</returns>
    /// <exception cref="ArgumentNullException">If identifier or verifier is null</exception>
    /// <exception cref="ArgumentException">If identifier is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">If a proof system with this identifier is already registered (Spec 003 FR-007)</exception>
    /// <remarks>
    /// <para>Duplicate identifiers are detected immediately and throw InvalidOperationException.</para>
    /// <code>
    /// var options = new ValidationOptions()
    ///     .AddProofSystem("paillier", new PaillierVerifier());
    /// 
    /// // This throws InvalidOperationException:
    /// options.AddProofSystem("paillier", new AnotherPaillierVerifier());
    /// </code>
    /// </remarks>
    public ValidationOptions AddProofSystem(string identifier, IProofSystemVerifier verifier)
    {
        if (identifier is null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be empty or whitespace.", nameof(identifier));
        }

        if (verifier is null)
        {
            throw new ArgumentNullException(nameof(verifier));
        }

        if (!proofSystems.TryAdd(identifier, verifier))
        {
            throw new InvalidOperationException(
                $"A proof system verifier with identifier '{identifier}' is already registered. Duplicate identifiers are not allowed (Spec 003 FR-007).");
        }

        return this;
    }

    /// <summary>
    /// Registers a custom statement handler.
    /// </summary>
    /// <param name="handler">Implementation of IStatementHandler</param>
    /// <returns>This instance for builder chaining</returns>
    /// <exception cref="ArgumentNullException">If handler is null</exception>
    /// <exception cref="InvalidOperationException">If a statement handler is already registered (Spec 003 FR-008)</exception>
    /// <remarks>
    /// <para>
    /// Statement handlers are stored in a collection and used during validation.
    /// Unlike proof systems (which have unique identifiers), statement handlers are checked
    /// for validation semantics match during validation execution.
    /// </para>
    /// <code>
    /// var options = new ValidationOptions()
    ///     .AddStatementHandler(new MyCustomStatementHandler());
    /// </code>
    /// </remarks>
    public ValidationOptions AddStatementHandler(IStatementHandler handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        // Check for duplicate handler types (same type registered twice)
        var handlerType = handler.GetType();
        if (statementHandlers.Any(h => h.GetType() == handlerType))
        {
            throw new InvalidOperationException(
                $"A statement handler of type '{handlerType.Name}' is already registered. Duplicate handler types are not allowed (Spec 003 FR-008).");
        }

        statementHandlers.Add(handler);
        return this;
    }
}
