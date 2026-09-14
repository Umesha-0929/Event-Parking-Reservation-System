using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueMediaConfiguration : IEntityTypeConfiguration<VenueMedia>
{
    public void Configure(EntityTypeBuilder<VenueMedia> builder)
    {
        builder.ToTable("VenueMedia");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => x.VenueId);
    }
}
