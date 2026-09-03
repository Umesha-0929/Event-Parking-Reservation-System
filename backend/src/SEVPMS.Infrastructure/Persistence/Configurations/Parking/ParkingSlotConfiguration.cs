using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingSlotConfiguration
    : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(
        EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable("ParkingSlots");

        builder.HasKey(slot => slot.Id);

        builder.Property(slot => slot.ParkingZoneId)
            .IsRequired();

        builder.Property(slot => slot.SlotCode)
            .IsRequired();

        builder.Property(slot => slot.X)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(slot => slot.Y)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(slot => slot.IsAccessible)
            .IsRequired();

        builder.Property(slot => slot.Status)
            .IsRequired();

        builder.Property(slot => slot.UpdatedAtUtc)
            .IsConcurrencyToken();
    }
}