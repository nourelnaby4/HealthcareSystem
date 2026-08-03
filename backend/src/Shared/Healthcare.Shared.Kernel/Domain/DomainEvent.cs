namespace Healthcare.Shared.Kernel.Domain;

/// <summary>
/// Base for domain events. Domain events are immutable facts describing something meaningful
/// that happened in the domain; aggregates raise them and the persistence layer converts them
/// into reliable integration events (Outbox) and side effects.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
