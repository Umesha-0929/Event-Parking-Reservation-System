using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;

public sealed class SeatSectionConfiguration : IEntityTypeConfiguration<SeatSection>
{
    public void Configure(EntityTypeBuilder<SeatSection> builder)
    {
        builder.ToTable("SeatSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.SeatingLayoutId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RowCount)
            .IsRequired();

        builder.Property(x => x.ColumnCount)
            .IsRequired();

        builder.Property(x => x.X)
            .HasPrecision(12, 2);

        builder.Property(x => x.Y)
            .HasPrecision(12, 2);

        builder.Property(x => x.Width)
            .HasPrecision(12, 2);

        builder.Property(x => x.Height)
            .HasPrecision(12, 2);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsAccessibleSection)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SeatingLayoutId,
            x.Code
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.DisplayOrder
        });
    }
}
