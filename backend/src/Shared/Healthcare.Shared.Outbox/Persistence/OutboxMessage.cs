namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// An outbound integration message stored in the same transaction as the domain change that
/// produced it. A hosted dispatcher reads pending rows and publishes them on the in-process bus;
/// at-least-once delivery; consumers dedupe via the Inbox. Never contains PHI beyond the contract
/// payload, and the payload is never written to application logs.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
