namespace Healthcare.Shared.Kernel.Domain;

/// <summary>Marker interface for all domain events raised by aggregates.</summary>
public interface IDomainEvent
{
    /// <summary>When the event occurred, in UTC.</summary>
    DateTimeOffset OccurredAtUtc { get; }
}
