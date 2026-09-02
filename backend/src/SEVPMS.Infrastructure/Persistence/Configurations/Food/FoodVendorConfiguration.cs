using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class FoodVendorConfiguration
    : IEntityTypeConfiguration<FoodVendor>
{
    public void Configure(
        EntityTypeBuilder<FoodVendor> builder)
    {
        builder.ToTable("FoodVendors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => x.OwnerUserId);
    }
}