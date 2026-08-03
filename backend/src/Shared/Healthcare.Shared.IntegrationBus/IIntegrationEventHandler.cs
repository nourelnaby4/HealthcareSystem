namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// Handles a single integration event type. Implementations are resolved by DI and invoked by
/// the in-process publisher. Handlers MUST be idempotent — the bus delivers at-least-once.
/// </summary>
/// <typeparam name="TEvent">The integration event contract type.</typeparam>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
