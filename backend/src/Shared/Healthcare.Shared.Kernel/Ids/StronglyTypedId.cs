namespace Healthcare.Shared.Kernel.Ids;

/// <summary>
/// Base type for all strongly-typed identifiers. Provides value equality
/// (via record semantics) and a consistent string representation.
/// </summary>
/// <typeparam name="TValue">The underlying primitive value (typically <see cref="Guid"/>).</typeparam>
public abstract record StronglyTypedId<TValue>(TValue Value) where TValue : notnull
{
    public override string ToString() => Value.ToString()!;
}
