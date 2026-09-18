using ModelProject.Entities.Enums;

namespace ModelProject.Services.WorkOrders;

// Single source of truth for the allowed status transitions (task PDF section 6), so the
// rule lives in exactly one place instead of being duplicated across controller/service/tests.
public static class WorkOrderStatusWorkflow
{
    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> AllowedTransitions = new()
    {
        [WorkOrderStatus.New] = new[] { WorkOrderStatus.Assigned },
        [WorkOrderStatus.Assigned] = new[] { WorkOrderStatus.InProgress },
        [WorkOrderStatus.InProgress] = new[] { WorkOrderStatus.Completed },
        [WorkOrderStatus.Completed] = Array.Empty<WorkOrderStatus>()
    };

    public static bool IsValidTransition(WorkOrderStatus from, WorkOrderStatus to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
