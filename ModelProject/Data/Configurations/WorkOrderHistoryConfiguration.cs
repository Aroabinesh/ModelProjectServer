using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModelProject.Entities;

namespace ModelProject.Data.Configurations;

public class WorkOrderHistoryConfiguration : IEntityTypeConfiguration<WorkOrderHistory>
{
    public void Configure(EntityTypeBuilder<WorkOrderHistory> builder)
    {
        builder.ToTable("WorkOrderHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.ChangedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Comments)
            .HasMaxLength(2000);

        builder.Property(h => h.ChangedAt).IsRequired();

        builder.HasOne(h => h.WorkOrder)
            .WithMany(w => w.History)
            .HasForeignKey(h => h.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.WorkOrderId, h.ChangedAt });
    }
}
