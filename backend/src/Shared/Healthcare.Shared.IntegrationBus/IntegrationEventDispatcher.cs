using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// In-process publisher that resolves all handlers registered for an event's contract type and
/// invokes them sequentially. At-least-once delivery is guaranteed by the Outbox; handlers must be
/// idempotent (dedupe via the Inbox).
/// </summary>
public sealed class IntegrationEventDispatcher : IIntegrationEventPublisher
{
    private readonly IServiceProvider _services;
    private readonly ILogger<IntegrationEventDispatcher> _logger;

    public IntegrationEventDispatcher(IServiceProvider services, ILogger<IntegrationEventDispatcher> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var eventType = @event.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

        // Resolve handlers in a scope so scoped services (DbContexts) are valid for the dispatch.
        using var scope = _services.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        var handlerCount = 0;
        foreach (var handler in handlers)
        {
            handlerCount++;
            var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync));
            if (handleMethod?.Invoke(handler, [@event, cancellationToken]) is Task task)
            {
                await task.ConfigureAwait(false);
            }
        }

        _logger.LogInformation(
            "Dispatched integration event {EventType} ({EventId}) to {HandlerCount} handler(s).",
            eventType.Name, @event.Id, handlerCount);
    }
}
