using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ModelProject.Common.Exceptions;
using ModelProject.Data;
using ModelProject.Dtos.WorkOrders;
using ModelProject.Entities.Enums;
using ModelProject.Services.WorkOrders;
using Xunit;

namespace ModelProject.Tests;

// Uses the EF Core InMemory provider against a fresh, uniquely-named database per test, seeded
// via the same SeedData.Seed(...) the real migration uses (applied automatically by
// EnsureCreated). Exercises WorkOrderService end to end: business rules, the status workflow,
// history creation, and optimistic-concurrency handling via RowVersion.
public class WorkOrderServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static WorkOrderService CreateService(ApplicationDbContext context)
        => new(context, NullLogger<WorkOrderService>.Instance);

    [Fact]
    public async Task CreateAsync_DerivesFacilityIdFromAsset()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = new CreateWorkOrderDto
        {
            AssetId = 4, // seeded asset belongs to facility 3
            Title = "New pump inspection",
            Description = "Routine inspection.",
            Priority = WorkOrderPriority.Low
        };

        var result = await service.CreateAsync(dto, "tester", CancellationToken.None);

        Assert.Equal(3, result.FacilityId);
        Assert.Equal(WorkOrderStatus.New.ToString(), result.Status);
    }

    [Fact]
    public async Task CreateAsync_UnknownAsset_ThrowsNotFoundException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = new CreateWorkOrderDto { AssetId = 999, Title = "Test", Description = "Test", Priority = WorkOrderPriority.Low };

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(dto, "tester", CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_CreatesHistoryRecord()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var workOrder = await service.GetByIdAsync(2, CancellationToken.None); // seeded: Assigned, technician 1

        var result = await service.ChangeStatusAsync(2, new ChangeStatusDto
        {
            NewStatus = WorkOrderStatus.InProgress,
            ChangedBy = "tester",
            Comments = "Starting work",
            RowVersion = workOrder.RowVersion
        }, CancellationToken.None);

        Assert.Equal(WorkOrderStatus.InProgress.ToString(), result.Status);

        var history = await service.GetHistoryAsync(2, CancellationToken.None);
        Assert.Contains(history, h => h.OldStatus == WorkOrderStatus.Assigned && h.NewStatus == WorkOrderStatus.InProgress);
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsInvalidStatusTransitionException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var workOrder = await service.GetByIdAsync(1, CancellationToken.None); // seeded: New

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() => service.ChangeStatusAsync(1, new ChangeStatusDto
        {
            NewStatus = WorkOrderStatus.Completed,
            ChangedBy = "tester",
            RowVersion = workOrder.RowVersion
        }, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatusAsync_ToAssignedWithoutTechnician_ThrowsBusinessRuleException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var workOrder = await service.GetByIdAsync(1, CancellationToken.None); // seeded: New, no technician

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ChangeStatusAsync(1, new ChangeStatusDto
        {
            NewStatus = WorkOrderStatus.Assigned,
            ChangedBy = "tester",
            RowVersion = workOrder.RowVersion
        }, CancellationToken.None));
    }

    [Fact]
    public async Task AssignTechnicianAsync_FromNew_TransitionsToAssignedAndRecordsHistory()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var workOrder = await service.GetByIdAsync(1, CancellationToken.None); // seeded: New, no technician

        var result = await service.AssignTechnicianAsync(1, new AssignTechnicianDto
        {
            TechnicianId = 3,
            ChangedBy = "tester",
            RowVersion = workOrder.RowVersion
        }, CancellationToken.None);

        Assert.Equal(WorkOrderStatus.Assigned.ToString(), result.Status);
        Assert.Equal(3, result.AssignedTechnicianId);

        var history = await service.GetHistoryAsync(1, CancellationToken.None);
        var entry = Assert.Single(history);
        Assert.Equal(WorkOrderStatus.New, entry.OldStatus);
        Assert.Equal(WorkOrderStatus.Assigned, entry.NewStatus);
    }

    [Fact]
    public async Task UpdateAsync_WithStaleRowVersion_ThrowsConcurrencyConflictException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Simulates two users opening the same work order: both read the same RowVersion.
        var userA = await service.GetByIdAsync(1, CancellationToken.None);
        var userB = await service.GetByIdAsync(1, CancellationToken.None);

        // SQL Server bumps ROWVERSION automatically on every write; the InMemory provider
        // does not, so we simulate "another process changed the row" explicitly here to
        // exercise the same ApplyRowVersion/SaveChanges concurrency-check code path that
        // runs in production (verified against a real SQL Server LocalDB instance separately;
        // see README "Concurrency").
        var tracked = await context.WorkOrders.FindAsync(new object[] { 1 }, CancellationToken.None);
        tracked!.RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        await context.SaveChangesAsync(CancellationToken.None);

        // User A "saves" using the RowVersion they read, which is now stale.
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => service.UpdateAsync(1, new UpdateWorkOrderDto
        {
            Title = "Updated by user A",
            Description = userA.Description,
            Priority = WorkOrderPriority.High,
            RowVersion = userA.RowVersion
        }, CancellationToken.None));

        // User B's RowVersion is equally stale.
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => service.UpdateAsync(1, new UpdateWorkOrderDto
        {
            Title = "Updated by user B",
            Description = userB.Description,
            Priority = WorkOrderPriority.Critical,
            RowVersion = userB.RowVersion
        }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_RemovesWorkOrder()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // WorkOrderHistories cascade-delete at the SQL Server FK level (ON DELETE CASCADE),
        // which the InMemory provider doesn't simulate for unloaded navigations, so that
        // behavior is verified against a real database instead (see README "Concurrency").
        await service.DeleteAsync(2, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(2, CancellationToken.None));
    }
}
