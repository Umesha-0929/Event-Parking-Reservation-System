using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class EventMenuItemConfiguration
    : IEntityTypeConfiguration<EventMenuItem>
{
    public void Configure(
        EntityTypeBuilder<EventMenuItem> builder)
    {
        builder.ToTable("EventMenuItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventFoodStallId)
            .IsRequired();

        builder.Property(x => x.MenuItemId)
            .IsRequired();

        builder.Property(x => x.EventPriceOverride)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.HasIndex(x => x.EventFoodStallId);
        builder.HasIndex(x => x.MenuItemId);
    }
}