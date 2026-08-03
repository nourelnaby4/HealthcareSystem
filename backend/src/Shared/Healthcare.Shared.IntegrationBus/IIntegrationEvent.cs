namespace Healthcare.Shared.IntegrationBus;

/// <summary>
/// Marker and envelope contract for an integration event published on the in-process bus.
/// Integration events are versioned cross-module contracts; consumers translate them into their
/// own models and must be idempotent (dedupe by <see cref="Id"/> via the Inbox).
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Message id = idempotency key.</summary>
    Guid Id { get; }

    /// <summary>Stable contract type name (e.g. <c>PatientAdmitted</c>).</summary>
    string Type { get; }

    /// <summary>Contract version.</summary>
    int Version { get; }

    /// <summary>When the event occurred, in UTC.</summary>
    DateTimeOffset OccurredAtUtc { get; }
}
