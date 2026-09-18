using System.ComponentModel.DataAnnotations;
using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.WorkOrders;

public class ChangeStatusDto
{
    [Required]
    public WorkOrderStatus NewStatus { get; init; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string ChangedBy { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Comments { get; init; }

    [Required]
    public string RowVersion { get; init; } = string.Empty;
}
