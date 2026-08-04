using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// Shared mapping for the Outbox table (the <c>integration</c> schema). A module's
/// <c>DbContext</c> calls <see cref="ApplyIntegrationOutbox"/> so outbox rows are written in the
/// same transaction as the domain change that produced them.
/// </summary>
public static class IntegrationModelBuilderExtensions
{
    public static ModelBuilder ApplyIntegrationOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(ConfigureOutboxMessage);
        return modelBuilder;
    }

    private static void ConfigureOutboxMessage(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "integration");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.HasIndex(x => x.ProcessedAt);
    }
}
