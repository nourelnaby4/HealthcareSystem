using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Healthcare.Shared.IntegrationBus;
using Healthcare.Shared.Kernel.Domain;
using Healthcare.Shared.Outbox.Events;
using Healthcare.Shared.Outbox.Persistence;

namespace Healthcare.Shared.Outbox;

public sealed class DomainEventOutboxInterceptor : SaveChangesInterceptor
{
    private readonly IntegrationEventCatalog _catalog;
    private readonly List<IIntegrationEventMapper> _mappers;

    public DomainEventOutboxInterceptor(IntegrationEventCatalog catalog, IEnumerable<IIntegrationEventMapper> mappers)
    {
        _catalog = catalog;
        _mappers = mappers.ToList();
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        QueueOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        QueueOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void QueueOutboxMessages(DbContext? context)
    {
        if (context is null || _mappers.Count == 0)
        {
            return;
        }

        var aggregates = context.ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IDomainEventSource>()
            .ToList();

        if (aggregates.Count == 0)
        {
            return;
        }

        var outbox = context.Set<OutboxMessage>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.PopDomainEvents())
            {
                foreach (var integrationEvent in _mappers.SelectMany(m => m.Map(domainEvent)))
                {
                    outbox.Add(new OutboxMessage
                    {
                        Id = integrationEvent.Id,
                        Type = integrationEvent.Type,
                        Payload = _catalog.Serialize(integrationEvent),
                        OccurredAt = integrationEvent.OccurredAtUtc,
                    });
                }
            }
        }
    }
}
