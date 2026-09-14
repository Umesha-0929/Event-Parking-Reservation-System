using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Venues;

public sealed class VenueConfiguration
    : IEntityTypeConfiguration<Venue>
{
    public void Configure(
        EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.AddressLine1)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(250);

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.District)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Country)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(30);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(256);

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.OwnerUserId);

        builder.HasIndex(x => x.Name);
    }
}