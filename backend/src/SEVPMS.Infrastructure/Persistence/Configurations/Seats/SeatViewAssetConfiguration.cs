using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;

public sealed class SeatViewAssetConfiguration : IEntityTypeConfiguration<SeatViewAsset>
{
    public void Configure(EntityTypeBuilder<SeatViewAsset> builder)
    {
        builder.ToTable("SeatViewAssets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MediaUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.ViewerType)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.RowLabel)
            .HasMaxLength(20);

        builder.Property(x => x.DefaultYaw)
            .HasPrecision(9, 4);

        builder.Property(x => x.DefaultPitch)
            .HasPrecision(9, 4);

        builder.Property(x => x.DefaultFov)
            .HasPrecision(9, 4);

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SeatId
        });

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SectionId,
            x.RowLabel
        });

        builder.HasIndex(x => new
        {
            x.EventId,
            x.SectionId
        });
    }
}
