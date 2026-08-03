using Microsoft.EntityFrameworkCore;
using Healthcare.Shared.Outbox.Persistence;

namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// Kernel-owned context for the Outbox/Inbox tables (the <c>integration</c> schema). The hosted
/// <see cref="OutboxDispatcher"/> uses it to read pending outbox rows. Module contexts co-map these
/// tables (via <see cref="IntegrationModelBuilderExtensions.ApplyIntegrationOutbox"/>) so writes
/// are transactional; this context is read/publish-side only.
/// </summary>
public sealed class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("integration");
        IntegrationModelBuilderExtensions.ConfigureOutboxMessage(modelBuilder.Entity<OutboxMessage>());
        IntegrationModelBuilderExtensions.ConfigureInboxMessage(modelBuilder.Entity<InboxMessage>());
    }
}
