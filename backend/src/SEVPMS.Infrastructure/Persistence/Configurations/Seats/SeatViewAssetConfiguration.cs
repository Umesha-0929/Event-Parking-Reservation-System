using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;
namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;
public sealed class SeatViewAssetConfiguration : IEntityTypeConfiguration<SeatViewAsset>
{
    public void Configure(EntityTypeBuilder<SeatViewAsset> b)
    {
        b.ToTable("SeatViewAssets"); b.HasKey(x => x.Id);
        b.Property(x => x.MediaUrl).HasMaxLength(1000).IsRequired(); b.Property(x => x.ViewerType).HasMaxLength(40).IsRequired();
        b.Property(x => x.DefaultYaw).HasPrecision(9, 4); b.Property(x => x.DefaultPitch).HasPrecision(9, 4); b.Property(x => x.DefaultFov).HasPrecision(9, 4);
        b.HasIndex(x => new { x.EventId, x.SeatId }); b.HasIndex(x => new { x.EventId, x.SectionId });
    }
}
