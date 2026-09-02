using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;

public sealed class SeatingLayoutConfiguration : IEntityTypeConfiguration<SeatingLayout>
{
    public void Configure(EntityTypeBuilder<SeatingLayout> builder)
    {
        builder.ToTable("SeatingLayouts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.StageType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RowCount)
            .IsRequired();

        builder.Property(x => x.ColumnCount)
            .IsRequired();

        builder.Property(x => x.CanvasWidth)
            .HasPrecision(12, 2);

        builder.Property(x => x.CanvasHeight)
            .HasPrecision(12, 2);

        builder.Property(x => x.StageX)
            .HasPrecision(12, 2);

        builder.Property(x => x.StageY)
            .HasPrecision(12, 2);

        builder.Property(x => x.StageWidth)
            .HasPrecision(12, 2);

        builder.Property(x => x.StageHeight)
            .HasPrecision(12, 2);

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.HasIndex(x => x.EventId)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.IsPublished
        });
    }
}
