using System.Collections.Concurrent;

namespace Healthcare.Shared.Kernel.Domain;

/// <summary>
/// Base class for all entities. Equality is identity-based (same type + same id).
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the entity.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>, IDomainEventSource
    where TId : notnull
{
    private readonly ConcurrentQueue<IDomainEvent> _domainEvents = new();

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected set; }

    /// <summary>Domain events raised by this entity since it was loaded/created.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Enqueue(domainEvent);
    }

    /// <summary>Removes and returns all pending domain events (called by the persistence interceptor).</summary>
    public IEnumerable<IDomainEvent> PopDomainEvents()
    {
        while (_domainEvents.TryDequeue(out var domainEvent))
        {
            yield return domainEvent;
        }
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
