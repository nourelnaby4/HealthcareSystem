namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// Publishes an integration event to all registered in-process handlers. Implementations are
/// expected to forward to the <see cref="IntegrationEventDispatcher"/>; the Outbox dispatcher calls
/// this after successfully reading an outbox row so delivery is reliable and transactional.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken);
}
