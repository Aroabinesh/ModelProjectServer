namespace ModelProject.Dtos.WorkOrders;

// Deliberately narrow: this is what renders one row of the work-orders table. Keeping it
// lean (vs. reusing WorkOrderDetailDto) keeps list-endpoint payloads small under pagination,
// per the task's "avoid unnecessarily large API responses" performance requirement.
public class WorkOrderListItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int FacilityId { get; init; }
    public string FacilityName { get; init; } = string.Empty;
    public int AssetId { get; init; }
    public string AssetCode { get; init; } = string.Empty;
    public int? AssignedTechnicianId { get; init; }
    public string? AssignedTechnicianName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
