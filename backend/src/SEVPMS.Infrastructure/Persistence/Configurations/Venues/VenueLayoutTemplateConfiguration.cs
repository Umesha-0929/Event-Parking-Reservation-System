using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueLayoutTemplateConfiguration : IEntityTypeConfiguration<VenueLayoutTemplate>
{
    public void Configure(EntityTypeBuilder<VenueLayoutTemplate> builder)
    {
        builder.ToTable("VenueLayoutTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.LayoutJson).IsRequired();
        builder.HasIndex(x => new { x.VenueId, x.Name, x.Version }).IsUnique();
    }
}
