using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class EventFoodStallConfiguration
    : IEntityTypeConfiguration<EventFoodStall>
{
    public void Configure(
        EntityTypeBuilder<EventFoodStall> builder)
    {
        builder.ToTable("EventFoodStalls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.VendorId)
            .IsRequired();

        builder.Property(x => x.StallName)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.OpensAtUtc)
            .IsRequired();

        builder.Property(x => x.ClosesAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.VendorId);
    }
}