namespace ModelProject.Dtos.WorkOrders;

public class WorkOrderDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public int AssetId { get; init; }
    public string AssetCode { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;

    public int FacilityId { get; init; }
    public string FacilityName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;

    public int? AssignedTechnicianId { get; init; }
    public string? AssignedTechnicianName { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    // Base64-encoded rowversion. The client must echo this back on PUT/assign/status calls
    // as the optimistic-concurrency token.
    public string RowVersion { get; init; } = string.Empty;
}
