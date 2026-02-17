// Spec 002 (Decision 5): Default clock implementation.

namespace Sigil.Sdk.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
