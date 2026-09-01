using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Tickets;
namespace SEVPMS.Infrastructure.Persistence.Configurations.Tickets;
public sealed class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> b)
    {
        b.ToTable("CheckIns"); b.HasKey(x => x.Id);
        b.Property(x => x.Gate).HasMaxLength(120); b.Property(x => x.Result).HasConversion<string>().HasMaxLength(32).IsRequired(); b.Property(x => x.Detail).HasMaxLength(300);
        b.HasIndex(x => new { x.TicketId, x.ScannedAtUtc }); b.HasIndex(x => new { x.EventId, x.ScannedAtUtc });
    }
}
