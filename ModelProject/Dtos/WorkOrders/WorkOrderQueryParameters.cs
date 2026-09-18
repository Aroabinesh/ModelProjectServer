using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.WorkOrders;

// Bound from the querystring on GET /api/workorders. Page/PageSize are clamped in their
// setters so a malicious or buggy caller can't request page size 0 or 500,000.
public class WorkOrderQueryParameters
{
    public string? Search { get; init; }
    public WorkOrderStatus? Status { get; init; }
    public WorkOrderPriority? Priority { get; init; }
    public int? FacilityId { get; init; }
    public int? TechnicianId { get; init; }

    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;

    private int _page = 1;
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value is < 1 or > 100 ? 20 : value;
    }
}
