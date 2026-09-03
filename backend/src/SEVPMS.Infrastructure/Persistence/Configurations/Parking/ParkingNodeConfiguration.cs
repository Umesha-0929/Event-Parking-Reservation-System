using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingNodeConfiguration
    : IEntityTypeConfiguration<ParkingNode>
{
    public void Configure(
        EntityTypeBuilder<ParkingNode> builder)
    {
        builder.ToTable("ParkingNodes");

        builder.HasKey(node => node.Id);

        builder.Property(node => node.VenueId)
            .IsRequired();

        builder.Property(node => node.NodeCode)
            .IsRequired();

        builder.Property(node => node.X)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(node => node.Y)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(node => node.NodeType)
            .IsRequired();
    }
}