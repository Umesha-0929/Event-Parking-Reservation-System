using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;
namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;
public sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> b)
    {
        b.ToTable("Seats"); b.HasKey(x => x.Id);
        b.Property(x => x.RowLabel).HasMaxLength(30).IsRequired();
        b.Property(x => x.SeatNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.X).HasPrecision(18, 4); b.Property(x => x.Y).HasPrecision(18, 4);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.EventId, x.SectionId, x.Status });
        b.HasIndex(x => new { x.EventId, x.SectionId, x.RowLabel, x.SeatNumber }).IsUnique();
    }
}
