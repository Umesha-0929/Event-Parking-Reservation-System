using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Food;

public sealed class MenuItemConfiguration
    : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(
        EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VendorId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(x => x.VendorId);
    }
}