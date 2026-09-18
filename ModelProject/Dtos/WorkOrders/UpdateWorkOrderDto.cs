using System.ComponentModel.DataAnnotations;
using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.WorkOrders;

// Covers title/description/priority only. AssetId is immutable after creation, and status /
// technician changes go through their own dedicated endpoints (PUT .../status, PUT .../assign)
// so each has its own validation and history/audit behavior instead of being folded in here.
public class UpdateWorkOrderDto
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public WorkOrderPriority Priority { get; init; }

    [Required]
    public string RowVersion { get; init; } = string.Empty;
}
