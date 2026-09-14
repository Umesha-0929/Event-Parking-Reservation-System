using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.CustomerUserId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CheckoutReference).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.CustomerUserId);
        builder.HasIndex(x => x.CheckoutReference).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
