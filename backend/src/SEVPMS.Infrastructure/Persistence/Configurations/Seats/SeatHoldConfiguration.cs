using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Seats;
namespace SEVPMS.Infrastructure.Persistence.Configurations.Seats;
public sealed class SeatHoldConfiguration : IEntityTypeConfiguration<SeatHold>
{
    public void Configure(EntityTypeBuilder<SeatHold> b)
    {
        b.ToTable("SeatHolds"); b.HasKey(x => x.Id);
        b.Property(x => x.HoldToken).HasMaxLength(80).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x => x.HoldToken);
        b.HasIndex(x => new { x.SeatId, x.ExpiresAtUtc, x.Status });
        b.HasIndex(x => new { x.SeatId, x.Status }).IsUnique().HasFilter("[Status] = 'Active'");
    }
}
