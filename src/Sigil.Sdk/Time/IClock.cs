// Spec 002 (Decision 5): Injectable clock for deterministic testing.

namespace Sigil.Sdk.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
