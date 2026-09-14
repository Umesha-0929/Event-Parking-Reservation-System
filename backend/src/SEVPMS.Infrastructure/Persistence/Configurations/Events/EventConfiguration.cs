using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Events;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Events;

public sealed class EventConfiguration
    : IEntityTypeConfiguration<Event>
{
    public void Configure(
        EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizerUserId)
            .IsRequired();

        builder.Property(x => x.VenueId)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(x => x.StartAtUtc)
            .IsRequired();

        builder.Property(x => x.EndAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(x => x.CategoryEntity)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizerUserId);

        builder.HasIndex(x => x.VenueId);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.StartAtUtc);

        builder.HasIndex(x => new
        {
            x.StartAtUtc,
            x.Status,
            x.CategoryId,
            x.VenueId
        });
    }
}