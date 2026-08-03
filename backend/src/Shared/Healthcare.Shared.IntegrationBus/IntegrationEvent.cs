namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// Base implementation for integration events. Deriving records set <see cref="Type"/> to the
/// simple type name and default <see cref="Version"/> to 1; override either when bumping a contract.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    /// <inheritdoc />
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public virtual string Type => GetType().Name;

    /// <inheritdoc />
    public virtual int Version => 1;

    /// <inheritdoc />
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
