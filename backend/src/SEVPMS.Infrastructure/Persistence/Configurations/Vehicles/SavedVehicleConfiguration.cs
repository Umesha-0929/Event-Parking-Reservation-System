using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Vehicles;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Vehicles;

public sealed class SavedVehicleConfiguration
    : IEntityTypeConfiguration<SavedVehicle>
{
    public void Configure(EntityTypeBuilder<SavedVehicle> builder)
    {
        builder.ToTable("SavedVehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.UserId)
            .IsRequired();

        builder.Property(vehicle => vehicle.Nickname)
            .IsRequired();

        builder.Property(vehicle => vehicle.RegistrationNo)
            .IsRequired();

        builder.Property(vehicle => vehicle.VehicleType)
            .IsRequired();

        builder.Property(vehicle => vehicle.IsDefault)
            .IsRequired();
    }
}