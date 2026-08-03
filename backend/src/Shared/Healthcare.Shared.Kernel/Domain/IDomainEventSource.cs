namespace Healthcare.Shared.Kernel.Domain;

/// <summary>
/// Non-generic accessor for an entity's pending domain events. Implemented by <see cref="Entity{TId}"/>
/// so the persistence layer can enumerate/pop events from any aggregate without knowing its id type.
/// </summary>
public interface IDomainEventSource
{
    /// <summary>Domain events currently queued on this entity (read-only view).</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Dequeues and returns all pending domain events.</summary>
    IEnumerable<IDomainEvent> PopDomainEvents();
}
