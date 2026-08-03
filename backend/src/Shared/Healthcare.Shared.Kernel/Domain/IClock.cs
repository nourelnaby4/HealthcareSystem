namespace Healthcare.Shared.Kernel.Domain;

/// <summary>
/// Abstraction over the system clock so domain logic that depends on "now" is deterministic
/// under test. Inject the clock rather than calling <see cref="DateTimeOffset.UtcNow"/> directly.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Default implementation backed by <see cref="DateTimeOffset.UtcNow"/>.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
