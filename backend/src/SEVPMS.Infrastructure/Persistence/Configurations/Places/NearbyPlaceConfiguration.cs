using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Places;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Places;

public sealed class NearbyPlaceConfiguration : IEntityTypeConfiguration<NearbyPlace>
{
    public void Configure(EntityTypeBuilder<NearbyPlace> builder)
    {
        builder.ToTable("NearbyPlaces");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(80);
        builder.Property(x => x.TagsCsv).HasMaxLength(1000);
        builder.Property(x => x.AudienceModesCsv).HasMaxLength(300);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.DistanceKm).HasPrecision(10, 2);
        builder.Property(x => x.Latitude).HasPrecision(10, 7);
        builder.Property(x => x.Longitude).HasPrecision(10, 7);
        builder.Property(x => x.DirectionsUrl).HasMaxLength(1000);

        builder.HasIndex(x => new { x.VenueId, x.IsActive });
        builder.HasIndex(x => new { x.VenueId, x.Category });
    }
}
