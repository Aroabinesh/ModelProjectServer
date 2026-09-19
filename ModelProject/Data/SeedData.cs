using Microsoft.EntityFrameworkCore;
using ModelProject.Entities;
using ModelProject.Entities.Enums;

namespace ModelProject.Data;

// Small, fixed demo dataset applied via migration (HasData). Dates are hard-coded rather than
// DateTime.UtcNow so the generated migration is deterministic and idempotent.
public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Acme Manufacturing" },
            new Customer { Id = 2, Name = "Northwind Logistics" }
        );

        modelBuilder.Entity<Facility>().HasData(
            new Facility { Id = 1, CustomerId = 1, Name = "Acme Plant 1", Location = "Chennai, IN" },
            new Facility { Id = 2, CustomerId = 1, Name = "Acme Plant 2", Location = "Coimbatore, IN" },
            new Facility { Id = 3, CustomerId = 2, Name = "Northwind Warehouse A", Location = "Bengaluru, IN" }
        );

        modelBuilder.Entity<Asset>().HasData(
            new Asset { Id = 1, FacilityId = 1, AssetCode = "AC-1001", Name = "Air Compressor #1", AssetType = "Compressor", Status = AssetStatus.Active },
            new Asset { Id = 2, FacilityId = 1, AssetCode = "CV-1002", Name = "Conveyor Belt A", AssetType = "Conveyor", Status = AssetStatus.Active },
            new Asset { Id = 3, FacilityId = 2, AssetCode = "HV-2001", Name = "HVAC Unit 3", AssetType = "HVAC", Status = AssetStatus.UnderMaintenance },
            new Asset { Id = 4, FacilityId = 3, AssetCode = "FL-3001", Name = "Forklift 7", AssetType = "Vehicle", Status = AssetStatus.Active }
        );

        // Password is "admin123" (see login screen demo credentials), hashed with BCrypt ahead
        // of time so the migration stays deterministic instead of re-hashing (and re-salting)
        // on every migration regeneration.
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$LDw7K2LrKIm8TYaksb2cHO5e9NB/6l2QBTuOOaLQ2ez9tWvhj9Ux6",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Technician>().HasData(
            new Technician { Id = 1, Name = "Arun Kumar", Email = "arun.kumar@example.com", IsActive = true, StartDate = new DateTime(2024, 1, 15) },
            new Technician { Id = 2, Name = "Priya Sharma", Email = "priya.sharma@example.com", IsActive = true, StartDate = new DateTime(2024, 3, 1) },
            new Technician { Id = 3, Name = "Ravi Verma", Email = "ravi.verma@example.com", IsActive = true, StartDate = new DateTime(2025, 6, 10) }
        );

        var seedDate = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<WorkOrder>().HasData(
            new WorkOrder
            {
                Id = 1,
                AssetId = 1,
                FacilityId = 1,
                Title = "Compressor pressure drop",
                Description = "Pressure drops below threshold after 20 minutes of operation.",
                Priority = WorkOrderPriority.High,
                Status = WorkOrderStatus.New,
                AssignedTechnicianId = null,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new WorkOrder
            {
                Id = 2,
                AssetId = 2,
                FacilityId = 1,
                Title = "Conveyor belt misalignment",
                Description = "Belt drifts to one side under load.",
                Priority = WorkOrderPriority.Medium,
                Status = WorkOrderStatus.Assigned,
                AssignedTechnicianId = 1,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new WorkOrder
            {
                Id = 3,
                AssetId = 3,
                FacilityId = 2,
                Title = "HVAC unit not cooling",
                Description = "Unit runs continuously without reaching the set temperature.",
                Priority = WorkOrderPriority.Critical,
                Status = WorkOrderStatus.InProgress,
                AssignedTechnicianId = 2,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        modelBuilder.Entity<WorkOrderHistory>().HasData(
            new WorkOrderHistory { Id = 1, WorkOrderId = 2, OldStatus = WorkOrderStatus.New, NewStatus = WorkOrderStatus.Assigned, ChangedBy = "system.seed", ChangedAt = seedDate, Comments = "Assigned to Arun Kumar." },
            new WorkOrderHistory { Id = 2, WorkOrderId = 3, OldStatus = WorkOrderStatus.New, NewStatus = WorkOrderStatus.Assigned, ChangedBy = "system.seed", ChangedAt = seedDate, Comments = "Assigned to Priya Sharma." },
            new WorkOrderHistory { Id = 3, WorkOrderId = 3, OldStatus = WorkOrderStatus.Assigned, NewStatus = WorkOrderStatus.InProgress, ChangedBy = "system.seed", ChangedAt = seedDate, Comments = "Technician started work." }
        );
    }
}
