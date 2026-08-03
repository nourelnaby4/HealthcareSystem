namespace Healthcare.Shared.Kernel.Domain;

/// <summary>
/// Base class for aggregate roots — the consistency boundary of a cluster of entities.
/// Aggregates enforce their invariants and raise domain events for cross-aggregate/
/// cross-module side effects.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the aggregate.</typeparam>
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : notnull
{
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    protected void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
