using System.Text.Json;
using System.Text.Json.Serialization;

namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// Maintains the catalog of known integration-event contract types and serializes/deserializes
/// them for the Outbox payload column. Outbox rows store only the contract <c>Type</c> name and a
/// JSON payload; the dispatcher resolves the CLR type here before publishing.
/// </summary>
public sealed class IntegrationEventCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly Dictionary<string, Type> _byTypeName = new(StringComparer.Ordinal);

    public void Register<TEvent>()
        where TEvent : IIntegrationEvent
    {
        var type = typeof(TEvent);
        _byTypeName[type.Name] = type;
    }

    public void Register(Type eventType)
    {
        if (!typeof(IIntegrationEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException($"{eventType.Name} does not implement {nameof(IIntegrationEvent)}.", nameof(eventType));
        }

        _byTypeName[eventType.Name] = eventType;
    }

    public Type Resolve(string typeName)
    {
        if (!_byTypeName.TryGetValue(typeName, out var type))
        {
            throw new InvalidOperationException(
                $"Integration event type '{typeName}' is not registered. Register it in the catalog at composition root.");
        }

        return type;
    }

    public string Serialize(IIntegrationEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return JsonSerializer.Serialize(@event, @event.GetType(), SerializerOptions);
    }

    public IIntegrationEvent Deserialize(string typeName, string payload)
    {
        var type = Resolve(typeName);
        return (IIntegrationEvent)(JsonSerializer.Deserialize(payload, type, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize integration event '{typeName}'."));
    }
}
