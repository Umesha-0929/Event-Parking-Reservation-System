using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;

public sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.SeatingLayoutId)
            .IsRequired();

        builder.Property(x => x.SectionId)
            .IsRequired();

        builder.Property(x => x.SeatCategoryId);

        builder.Property(x => x.RowLabel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RowNumber)
            .IsRequired();

        builder.Property(x => x.ColumnNumber)
            .IsRequired();

        builder.Property(x => x.SeatNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.X)
            .HasPrecision(12, 2);

        builder.Property(x => x.Y)
            .HasPrecision(12, 2);

        builder.Property(x => x.TicketTypeId);

        builder.Property(x => x.IsAccessible)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SeatViewAssetId);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SectionId,
            x.Status
        });

        builder.HasIndex(x => new
        {
            x.SeatingLayoutId,
            x.SectionId,
            x.SeatCategoryId
        });

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SectionId,
            x.RowNumber,
            x.ColumnNumber
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SectionId,
            x.RowLabel,
            x.SeatNumber
        })
        .IsUnique();
    }
}
