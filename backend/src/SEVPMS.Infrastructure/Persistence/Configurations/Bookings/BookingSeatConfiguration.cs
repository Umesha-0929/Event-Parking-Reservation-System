using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Bookings;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Bookings;

public sealed class BookingSeatConfiguration : IEntityTypeConfiguration<BookingSeat>
{
    public void Configure(EntityTypeBuilder<BookingSeat> builder)
    {
        builder.ToTable("BookingSeats");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.SeatId).IsRequired();

        builder.HasIndex(x => new { x.BookingId, x.SeatId }).IsUnique();
        builder.HasIndex(x => x.SeatId);
    }
}
