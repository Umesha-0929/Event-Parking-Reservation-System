using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Receipts;

public sealed class ReceiptDeliveryConfiguration : IEntityTypeConfiguration<ReceiptDelivery>
{
    public void Configure(EntityTypeBuilder<ReceiptDelivery> builder)
    {
        builder.ToTable("ReceiptDeliveries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Channel).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DestinationMasked).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ReceiptId, x.Channel }).IsUnique();
        builder.HasIndex(x => x.CustomerUserId);
        builder.HasIndex(x => x.Status);
    }
}
