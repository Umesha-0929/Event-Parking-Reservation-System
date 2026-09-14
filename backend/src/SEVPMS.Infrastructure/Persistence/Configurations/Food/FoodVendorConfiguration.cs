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
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(x => x.OwnerUserId);
    }
}