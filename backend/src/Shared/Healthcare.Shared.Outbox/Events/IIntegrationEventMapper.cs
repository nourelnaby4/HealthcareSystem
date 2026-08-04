using Healthcare.Shared.IntegrationBus;
using Healthcare.Shared.Kernel.Domain;

namespace Healthcare.Shared.Outbox.Events;

public interface IIntegrationEventMapper
{
    IEnumerable<IIntegrationEvent> Map(IDomainEvent domainEvent);
}

public abstract class IntegrationEventMapper<TDomainEvent> : IIntegrationEventMapper
    where TDomainEvent : IDomainEvent
{
    IEnumerable<IIntegrationEvent> IIntegrationEventMapper.Map(IDomainEvent domainEvent)
        => domainEvent is TDomainEvent typed ? Map(typed) : Enumerable.Empty<IIntegrationEvent>();

    protected abstract IEnumerable<IIntegrationEvent> Map(TDomainEvent domainEvent);
}
