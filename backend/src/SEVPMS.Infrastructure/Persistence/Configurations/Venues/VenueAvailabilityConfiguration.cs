using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueAvailabilityConfiguration : IEntityTypeConfiguration<VenueAvailability>
{
    public void Configure(EntityTypeBuilder<VenueAvailability> builder)
    {
        builder.ToTable("VenueAvailability");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.StartAtUtc).IsRequired();
        builder.Property(x => x.EndAtUtc).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasIndex(x => new { x.VenueId, x.StartAtUtc, x.EndAtUtc });
    }
}
