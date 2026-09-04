using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Waitlists;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Waitlists;

public sealed class WaitlistEntryConfiguration
    : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(
        EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("WaitlistEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.CustomerUserId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.EventId);

        builder.HasIndex(x => x.CustomerUserId);

        builder.HasIndex(x => new
        {
            x.EventId,
            x.CustomerUserId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.Status,
            x.CreatedAtUtc
        });
    }
}