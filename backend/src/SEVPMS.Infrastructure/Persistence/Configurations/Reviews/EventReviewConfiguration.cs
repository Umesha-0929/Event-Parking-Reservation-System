using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEVPMS.Domain.Entities.Reviews;

namespace SEVPMS.Infrastructure.Persistence.Configurations.Reviews;

public sealed class EventReviewConfiguration
    : IEntityTypeConfiguration<EventReview>
{
    public void Configure(
        EntityTypeBuilder<EventReview> builder)
    {
        builder.ToTable("EventReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.CustomerUserId)
            .IsRequired();

        builder.Property(x => x.BookingId)
            .IsRequired();

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.EventId);

        builder.HasIndex(x => x.CustomerUserId);

        builder.HasIndex(x => x.BookingId)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.CustomerUserId
        })
        .IsUnique();
    }
}