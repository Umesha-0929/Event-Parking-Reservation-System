using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Receipts;

public sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PaymentId).IsRequired();
        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.CustomerUserId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IssuedAtUtc).IsRequired();

        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.HasIndex(x => x.PaymentId).IsUnique();
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.CustomerUserId);
    }
}
