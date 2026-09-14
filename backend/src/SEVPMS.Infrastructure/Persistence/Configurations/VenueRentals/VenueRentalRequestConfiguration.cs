using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.VenueRentals;

namespace SEVPMS.Infrastructure.Persistence.Configurations.VenueRentals;

public sealed class VenueRentalRequestConfiguration
    : IEntityTypeConfiguration<VenueRentalRequest>
{
    public void Configure(EntityTypeBuilder<VenueRentalRequest> builder)
    {
        builder.ToTable("VenueRentalRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizerUserId).IsRequired();
        builder.Property(x => x.VenueId).IsRequired();
        builder.Property(x => x.StartAtUtc).IsRequired();
        builder.Property(x => x.EndAtUtc).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.OfferedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.OwnerMessage).HasMaxLength(1000);

        builder.HasIndex(x => x.OrganizerUserId);
        builder.HasIndex(x => x.VenueId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.VenueId, x.StartAtUtc, x.EndAtUtc });
    }
}
