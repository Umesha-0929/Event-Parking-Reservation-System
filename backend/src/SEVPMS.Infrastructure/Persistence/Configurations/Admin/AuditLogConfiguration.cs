using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Admin;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Admin;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(120);
        builder.Property(x => x.BeforeSummary).HasMaxLength(2000);
        builder.Property(x => x.AfterSummary).HasMaxLength(2000);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.HasIndex(x => x.ActorUserId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
