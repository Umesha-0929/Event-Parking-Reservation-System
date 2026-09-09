using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingZoneConfiguration
    : IEntityTypeConfiguration<ParkingZone>
{
    public void Configure(EntityTypeBuilder<ParkingZone> builder)
    {
        builder.ToTable("ParkingZones");

        builder.HasKey(zone => zone.Id);

        builder.Property(zone => zone.VenueId)
            .IsRequired();

        builder.Property(zone => zone.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(zone => zone.Level)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(zone => zone.EntranceName)
            .HasMaxLength(160)
            .IsRequired();
    }
}