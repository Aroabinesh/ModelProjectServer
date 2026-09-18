using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.WorkOrders;

public class WorkOrderHistoryDto
{
    public int Id { get; init; }
    public WorkOrderStatus OldStatus { get; init; }
    public WorkOrderStatus NewStatus { get; init; }
    public string ChangedBy { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; }
    public string? Comments { get; init; }
}
