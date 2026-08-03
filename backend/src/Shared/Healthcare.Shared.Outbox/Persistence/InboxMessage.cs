namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// Records that an incoming integration message id has been processed. Consumers insert a row
/// before applying an effect so at-least-once redelivery yields exactly-once effect (idempotency).
/// </summary>
public sealed class InboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }
}
