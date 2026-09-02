using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Parking;

public sealed class ParkingEdgeConfiguration
    : IEntityTypeConfiguration<ParkingEdge>
{
    public void Configure(EntityTypeBuilder<ParkingEdge> builder)
    {
        builder.ToTable("ParkingEdges");

        builder.HasKey(edge => edge.Id);

        builder.Property(edge => edge.VenueId)
            .IsRequired();

        builder.Property(edge => edge.FromNodeId)
            .IsRequired();

        builder.Property(edge => edge.ToNodeId)
            .IsRequired();

        builder.Property(edge => edge.Cost)
            .IsRequired();

        builder.Property(edge => edge.IsBidirectional)
            .IsRequired();

        builder.Property(edge => edge.IsAccessible)
            .IsRequired();

        builder.Property(edge => edge.IsBlocked)
            .IsRequired();
    }
}