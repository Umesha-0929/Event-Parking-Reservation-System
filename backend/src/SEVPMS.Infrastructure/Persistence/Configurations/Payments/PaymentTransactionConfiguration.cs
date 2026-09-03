using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(60).IsRequired();
        builder.Property(x => x.ProviderReference).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PayloadHash).HasMaxLength(128);
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.ProviderReference);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
