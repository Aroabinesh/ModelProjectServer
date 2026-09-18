using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModelProject.Entities;

namespace ModelProject.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(w => w.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        // SQL Server ROWVERSION column, auto-updated by the engine on every write.
        // EF Core uses it as the optimistic concurrency token (see IWorkOrderService).
        builder.Property(w => w.RowVersion).IsRowVersion();

        builder.HasOne(w => w.Asset)
            .WithMany(a => a.WorkOrders)
            .HasForeignKey(w => w.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Facility)
            .WithMany(f => f.WorkOrders)
            .HasForeignKey(w => w.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.AssignedTechnician)
            .WithMany(t => t.WorkOrders)
            .HasForeignKey(w => w.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.Restrict);

        // Covers: SELECT ... FROM WorkOrders WHERE FacilityId = @f AND Status = @s ORDER BY CreatedAt DESC
        // (the query from the SQL Server challenge, task PDF section 11). CreatedAt DESC matches the
        // index order so SQL Server can satisfy the ORDER BY without an extra sort operator.
        builder.HasIndex(w => new { w.FacilityId, w.Status, w.CreatedAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("IX_WorkOrders_FacilityId_Status_CreatedAt");

        builder.HasIndex(w => w.AssetId);
        builder.HasIndex(w => w.AssignedTechnicianId);
        builder.HasIndex(w => w.Priority);
        builder.HasIndex(w => w.CreatedAt);
    }
}
