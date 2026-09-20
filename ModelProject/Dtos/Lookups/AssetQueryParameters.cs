using ModelProject.Entities.Enums;

namespace ModelProject.Dtos.Lookups;

// Bound from the querystring on GET /api/assets. Page/PageSize are clamped in their setters
// for the same reason as WorkOrderQueryParameters: with 100k+ assets, an unbounded page size
// would load the entire table into memory and over the wire on every request.
public class AssetQueryParameters
{
    public string? Search { get; init; }
    public int? FacilityId { get; init; }
    public AssetStatus? Status { get; init; }

    public string SortBy { get; init; } = "AssetCode";
    public bool SortDescending { get; init; }

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
