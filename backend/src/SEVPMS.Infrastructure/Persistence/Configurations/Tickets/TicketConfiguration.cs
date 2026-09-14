using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Tickets;
namespace SEVPMS.Infrastructure.Persistence.Configurations.Tickets;
public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("Tickets"); b.HasKey(x => x.Id);
        b.Property(x => x.TicketNo).HasMaxLength(64).IsRequired(); b.Property(x => x.QrTokenHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x => x.TicketNo).IsUnique(); b.HasIndex(x => x.QrTokenHash).IsUnique(); b.HasIndex(x => x.BookingId);
    }
}
