using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingNodeConfiguration
    : IEntityTypeConfiguration<ParkingNode>
{
    public void Configure(EntityTypeBuilder<ParkingNode> builder)
    {
        builder.ToTable("ParkingNodes");

        builder.HasKey(node => node.Id);

        builder.Property(node => node.VenueId)
            .IsRequired();

        builder.Property(node => node.NodeCode)
            .IsRequired();

        builder.Property(node => node.X)
            .IsRequired();

        builder.Property(node => node.Y)
            .IsRequired();

        builder.Property(node => node.NodeType)
            .IsRequired();
    }
}