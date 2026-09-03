using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueRateConfiguration : IEntityTypeConfiguration<VenueRate>
{
    public void Configure(EntityTypeBuilder<VenueRate> builder)
    {
        builder.ToTable("VenueRates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.RateType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.HasIndex(x => new { x.VenueId, x.IsActive });
    }
}
