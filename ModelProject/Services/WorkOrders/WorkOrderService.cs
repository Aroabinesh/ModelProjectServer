using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ModelProject.Common.Exceptions;
using ModelProject.Data;
using ModelProject.Dtos.Common;
using ModelProject.Dtos.WorkOrders;
using ModelProject.Entities;
using ModelProject.Entities.Enums;

namespace ModelProject.Services.WorkOrders;

public class WorkOrderService : IWorkOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(ApplicationDbContext db, ILogger<WorkOrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Expression tree (not a compiled method) so EF Core can translate it into a single SQL
    // SELECT with JOINs against Asset/Facility/Customer/Technician instead of issuing separate
    // round-trips per navigation (avoids N+1; see README "Include vs projection").
    private static readonly Expression<Func<WorkOrder, WorkOrderDetailDto>> ToDetailDto = w => new WorkOrderDetailDto
    {
        Id = w.Id,
        Title = w.Title,
        Description = w.Description,
        Priority = w.Priority.ToString(),
        Status = w.Status.ToString(),
        AssetId = w.AssetId,
        AssetCode = w.Asset!.AssetCode,
        AssetName = w.Asset!.Name,
        FacilityId = w.FacilityId,
        FacilityName = w.Facility!.Name,
        CustomerName = w.Facility!.Customer!.Name,
        AssignedTechnicianId = w.AssignedTechnicianId,
        AssignedTechnicianName = w.AssignedTechnician == null ? null : w.AssignedTechnician.Name,
        ScheduledStartDate = w.ScheduledStartDate,
        ScheduledEndDate = w.ScheduledEndDate,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        RowVersion = Convert.ToBase64String(w.RowVersion)
    };

    public async Task<PagedResult<WorkOrderListItemDto>> GetListAsync(WorkOrderQueryParameters query, CancellationToken cancellationToken)
    {
        var workOrders = _db.WorkOrders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            workOrders = workOrders.Where(w =>
                EF.Functions.Like(w.Title, $"%{term}%") || EF.Functions.Like(w.Description, $"%{term}%"));
        }

        if (query.Status.HasValue)
            workOrders = workOrders.Where(w => w.Status == query.Status.Value);

        if (query.Priority.HasValue)
            workOrders = workOrders.Where(w => w.Priority == query.Priority.Value);

        if (query.FacilityId.HasValue)
            workOrders = workOrders.Where(w => w.FacilityId == query.FacilityId.Value);

        if (query.TechnicianId.HasValue)
            workOrders = workOrders.Where(w => w.AssignedTechnicianId == query.TechnicianId.Value);

        workOrders = ApplySort(workOrders, query.SortBy, query.SortDescending);

        // Two indexed queries (count + page) rather than loading everything into memory and
        // paging client-side — required given the "up to 5 million work orders" assumption.
        var totalCount = await workOrders.LongCountAsync(cancellationToken);

        var items = await workOrders
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(w => new WorkOrderListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Priority = w.Priority.ToString(),
                Status = w.Status.ToString(),
                FacilityId = w.FacilityId,
                FacilityName = w.Facility!.Name,
                AssetId = w.AssetId,
                AssetCode = w.Asset!.AssetCode,
                AssignedTechnicianId = w.AssignedTechnicianId,
                AssignedTechnicianName = w.AssignedTechnician == null ? null : w.AssignedTechnician.Name,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrderListItemDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<WorkOrder> ApplySort(IQueryable<WorkOrder> query, string sortBy, bool descending)
    {
        // Whitelisted switch rather than a dynamic "OrderBy(sortBy)" string - keeps sorting
        // safe from injection and guarantees every accepted value maps to an indexed column.
        Expression<Func<WorkOrder, object>> keySelector = sortBy.Trim().ToLowerInvariant() switch
        {
            "title" => w => w.Title,
            "priority" => w => w.Priority,
            "status" => w => w.Status,
            "updatedat" => w => w.UpdatedAt,
            _ => w => w.CreatedAt
        };

        return descending
            ? query.OrderByDescending(keySelector).ThenByDescending(w => w.Id)
            : query.OrderBy(keySelector).ThenBy(w => w.Id);
    }

    public async Task<WorkOrderDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var dto = await _db.WorkOrders.AsNoTracking()
            .Where(w => w.Id == id)
            .Select(ToDetailDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException($"Work order {id} was not found.");
    }

    public async Task<WorkOrderDetailDto> CreateAsync(CreateWorkOrderDto dto, string createdBy, CancellationToken cancellationToken)
    {
        var asset = await _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AssetId, cancellationToken)
            ?? throw new NotFoundException($"Asset {dto.AssetId} was not found.");

        if (dto.AssignedTechnicianId.HasValue)
        {
            var technicianExists = await _db.Technicians.AsNoTracking()
                .AnyAsync(t => t.Id == dto.AssignedTechnicianId.Value, cancellationToken);
            if (!technicianExists)
                throw new NotFoundException($"Technician {dto.AssignedTechnicianId} was not found.");
        }

        var now = DateTime.UtcNow;
        var workOrder = new WorkOrder
        {
            AssetId = asset.Id,
            // Derived from the asset server-side, never trusted from the client, so a caller
            // can't point a work order at a facility it has no relationship to.
            FacilityId = asset.FacilityId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority,
            Status = WorkOrderStatus.New,
            AssignedTechnicianId = dto.AssignedTechnicianId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Work order {WorkOrderId} created for asset {AssetId} by {CreatedBy}",
            workOrder.Id, workOrder.AssetId, createdBy);

        return await GetByIdAsync(workOrder.Id, cancellationToken);
    }

    public async Task<WorkOrderDetailDto> UpdateAsync(int id, UpdateWorkOrderDto dto, CancellationToken cancellationToken)
    {
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Work order {id} was not found.");

        ApplyRowVersion(workOrder, dto.RowVersion);

        workOrder.Title = dto.Title.Trim();
        workOrder.Description = dto.Description.Trim();
        workOrder.Priority = dto.Priority;
        workOrder.UpdatedAt = DateTime.UtcNow;

        await SaveWithConcurrencyHandlingAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Work order {id} was not found.");

        // WorkOrderHistories cascade-delete with the parent (see WorkOrderHistoryConfiguration).
        _db.WorkOrders.Remove(workOrder);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Work order {WorkOrderId} deleted.", id);
    }

    public async Task<WorkOrderDetailDto> AssignTechnicianAsync(int id, AssignTechnicianDto dto, CancellationToken cancellationToken)
    {
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Work order {id} was not found.");

        if (workOrder.Status == WorkOrderStatus.Completed)
            throw new BusinessRuleException("Cannot reassign a technician on a completed work order.");

        var technicianExists = await _db.Technicians.AsNoTracking()
            .AnyAsync(t => t.Id == dto.TechnicianId && t.IsActive, cancellationToken);
        if (!technicianExists)
            throw new NotFoundException($"Active technician {dto.TechnicianId} was not found.");

        if (dto.ScheduledStartDate.HasValue && dto.ScheduledEndDate.HasValue
            && dto.ScheduledEndDate.Value.Date < dto.ScheduledStartDate.Value.Date)
        {
            throw new BusinessRuleException("Scheduled end date cannot be before the scheduled start date.");
        }

        ApplyRowVersion(workOrder, dto.RowVersion);

        var previousStatus = workOrder.Status;
        workOrder.AssignedTechnicianId = dto.TechnicianId;
        workOrder.ScheduledStartDate = dto.ScheduledStartDate;
        workOrder.ScheduledEndDate = dto.ScheduledEndDate;
        workOrder.UpdatedAt = DateTime.UtcNow;

        // Assigning a technician to a brand-new work order is what moves it into the
        // "Assigned" state (task PDF section 6). Reassigning later on doesn't change status.
        if (previousStatus == WorkOrderStatus.New)
        {
            workOrder.Status = WorkOrderStatus.Assigned;
            _db.WorkOrderHistories.Add(new WorkOrderHistory
            {
                WorkOrderId = workOrder.Id,
                OldStatus = previousStatus,
                NewStatus = WorkOrderStatus.Assigned,
                ChangedBy = dto.ChangedBy.Trim(),
                ChangedAt = workOrder.UpdatedAt,
                Comments = $"Technician {dto.TechnicianId} assigned."
            });
        }

        await SaveWithConcurrencyHandlingAsync(cancellationToken);

        _logger.LogInformation("Technician {TechnicianId} assigned to work order {WorkOrderId} by {ChangedBy}",
            dto.TechnicianId, id, dto.ChangedBy);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<WorkOrderDetailDto> ChangeStatusAsync(int id, ChangeStatusDto dto, CancellationToken cancellationToken)
    {
        var workOrder = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Work order {id} was not found.");

        if (!WorkOrderStatusWorkflow.IsValidTransition(workOrder.Status, dto.NewStatus))
        {
            throw new InvalidStatusTransitionException(
                $"Cannot change status from '{workOrder.Status}' to '{dto.NewStatus}'. " +
                "Allowed transitions: New -> Assigned -> InProgress -> Completed.");
        }

        if (dto.NewStatus == WorkOrderStatus.Assigned && workOrder.AssignedTechnicianId is null)
        {
            throw new BusinessRuleException("Assign a technician before moving the work order to 'Assigned'.");
        }

        ApplyRowVersion(workOrder, dto.RowVersion);

        var previousStatus = workOrder.Status;
        workOrder.Status = dto.NewStatus;
        workOrder.UpdatedAt = DateTime.UtcNow;

        _db.WorkOrderHistories.Add(new WorkOrderHistory
        {
            WorkOrderId = workOrder.Id,
            OldStatus = previousStatus,
            NewStatus = dto.NewStatus,
            ChangedBy = dto.ChangedBy.Trim(),
            ChangedAt = workOrder.UpdatedAt,
            Comments = dto.Comments?.Trim()
        });

        await SaveWithConcurrencyHandlingAsync(cancellationToken);

        _logger.LogInformation("Work order {WorkOrderId} status changed {OldStatus} -> {NewStatus} by {ChangedBy}",
            workOrder.Id, previousStatus, dto.NewStatus, dto.ChangedBy);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrderHistoryDto>> GetHistoryAsync(int id, CancellationToken cancellationToken)
    {
        var exists = await _db.WorkOrders.AsNoTracking().AnyAsync(w => w.Id == id, cancellationToken);
        if (!exists)
            throw new NotFoundException($"Work order {id} was not found.");

        return await _db.WorkOrderHistories.AsNoTracking()
            .Where(h => h.WorkOrderId == id)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new WorkOrderHistoryDto
            {
                Id = h.Id,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt,
                Comments = h.Comments
            })
            .ToListAsync(cancellationToken);
    }

    // Sets the RowVersion the client last saw as the EF "original value" for this tracked
    // entity, so SaveChanges emits `WHERE Id = @id AND RowVersion = @clientRowVersion`. If
    // another user updated the row in between, zero rows match and EF throws
    // DbUpdateConcurrencyException - handled below as a 409, never a silent overwrite.
    private void ApplyRowVersion(WorkOrder workOrder, string rowVersionBase64)
    {
        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(rowVersionBase64);
        }
        catch (FormatException)
        {
            throw new BusinessRuleException("RowVersion must be a valid base64-encoded value.");
        }

        _db.Entry(workOrder).Property(w => w.RowVersion).OriginalValue = rowVersion;
    }

    private async Task SaveWithConcurrencyHandlingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while saving work order changes.");
            throw new ConcurrencyConflictException(
                "This work order was modified by another user after you loaded it. Reload the latest version and try again.");
        }
    }
}
