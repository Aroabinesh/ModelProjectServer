using System.ComponentModel.DataAnnotations;
using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.WorkOrders;

public class CreateWorkOrderDto
{
    [Required]
    public int AssetId { get; init; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public WorkOrderPriority Priority { get; init; }

    public int? AssignedTechnicianId { get; init; }

    // Optional audit label for logging; not persisted (WorkOrder has no CreatedBy column
    // per the task's data model). Defaults to "unknown" in the service when omitted.
    public string? CreatedBy { get; init; }
}
