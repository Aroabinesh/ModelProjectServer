using ModelProject.Entities.Enums;

namespace ModelProject.Entities;

public class WorkOrderHistory
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }

    public WorkOrderStatus OldStatus { get; set; }
    public WorkOrderStatus NewStatus { get; set; }

    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Comments { get; set; }

    public WorkOrder? WorkOrder { get; set; }
}
