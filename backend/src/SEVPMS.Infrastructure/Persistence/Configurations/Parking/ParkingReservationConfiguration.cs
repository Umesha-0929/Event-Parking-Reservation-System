using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingReservationConfiguration
    : IEntityTypeConfiguration<ParkingReservation>
{
    public void Configure(
        EntityTypeBuilder<ParkingReservation> builder)
    {
        builder.ToTable("ParkingReservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingId)
            .IsRequired();

        builder.Property(x => x.ParkingSlotId)
            .IsRequired();

        builder.Property(x => x.VehicleRegSnapshot)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ReservedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.BookingId);

        builder.HasIndex(x => new
        {
            x.ParkingSlotId,
            x.Status
        });
    }
}