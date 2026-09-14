using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueFacilityLinkConfiguration : IEntityTypeConfiguration<VenueFacilityLink>
{
    public void Configure(EntityTypeBuilder<VenueFacilityLink> builder)
    {
        builder.ToTable("VenueFacilityLinks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.FacilityId).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasIndex(x => new { x.VenueId, x.FacilityId }).IsUnique();
    }
}
