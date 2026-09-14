using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Bookings;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Bookings;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerUserId).IsRequired();
        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.HoldToken).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.BookingNumber).IsUnique();
        builder.HasIndex(x => x.CustomerUserId);
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.Status);
    }
}
