using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// Shared mapping for the Outbox/Inbox tables so a module's <c>DbContext</c> and the kernel's
/// <see cref="IntegrationDbContext"/> map the identical <c>integration</c>-schema tables. Module
/// contexts include these in their model so outbox rows are written in the same transaction as the
/// domain change that produced them.
/// </summary>
public static class IntegrationModelBuilderExtensions
{
    public static ModelBuilder ApplyIntegrationOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(ConfigureOutboxMessage);
        modelBuilder.Entity<InboxMessage>(ConfigureInboxMessage);
        return modelBuilder;
    }

    internal static void ConfigureOutboxMessage(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "integration");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(2048);
        builder.Property(x => x.ProcessedAt).HasIndex(); // implicit; explicit below
        builder.HasIndex(x => x.ProcessedAt);
    }

    internal static void ConfigureInboxMessage(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages", "integration");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
