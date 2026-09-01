using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;

public sealed class SeatCategoryConfiguration : IEntityTypeConfiguration<SeatCategory>
{
    public void Configure(EntityTypeBuilder<SeatCategory> builder)
    {
        builder.ToTable("SeatCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.SeatingLayoutId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SeatingLayoutId,
            x.Code
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.DisplayOrder
        });
    }
}
