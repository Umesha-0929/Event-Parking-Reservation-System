using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class FoodOrderItemConfiguration
    : IEntityTypeConfiguration<FoodOrderItem>
{
    public void Configure(
        EntityTypeBuilder<FoodOrderItem> builder)
    {
        builder.ToTable("FoodOrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FoodOrderId)
            .IsRequired();

        builder.Property(x => x.MenuItemId)
            .IsRequired();

        builder.Property(x => x.ItemNameSnapshot)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.LineTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasIndex(x => x.FoodOrderId);
        builder.HasIndex(x => x.MenuItemId);
    }
}