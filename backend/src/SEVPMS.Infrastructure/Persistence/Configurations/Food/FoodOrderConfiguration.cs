using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class FoodOrderConfiguration
    : IEntityTypeConfiguration<FoodOrder>
{
    public void Configure(
        EntityTypeBuilder<FoodOrder> builder)
    {
        builder.ToTable("FoodOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNo)
            .IsRequired();

        builder.HasIndex(x => x.OrderNo)
            .IsUnique();

        builder.Property(x => x.CustomerUserId)
            .IsRequired();

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.EventFoodStallId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FulfillmentType)
            .IsRequired();

        builder.Property(x => x.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.CustomerUserId);
        builder.HasIndex(x => x.EventFoodStallId);

        builder.HasIndex(x => new
        {
            x.EventId,
            x.Status,
            x.CreatedAtUtc
        });
    }
}