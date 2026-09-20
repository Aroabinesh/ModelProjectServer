using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ModelProject.Data;
using ModelProject.Dtos.Common;
using ModelProject.Dtos.Lookups;
using ModelProject.Entities;

namespace ModelProject.Services.Lookups;

// Small, mostly-static reference tables that back the React app's dropdowns/filters
// (customers, facilities, assets, technicians). Kept separate from IWorkOrderService since
// they're a different concern (read-only reference data vs. the work order aggregate).
public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _db;

    public LookupService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken)
        => await _db.Customers.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FacilityDto>> GetFacilitiesAsync(int? customerId, CancellationToken cancellationToken)
    {
        var facilities = _db.Facilities.AsNoTracking().AsQueryable();

        if (customerId.HasValue)
            facilities = facilities.Where(f => f.CustomerId == customerId.Value);

        return await facilities
            .OrderBy(f => f.Name)
            .Select(f => new FacilityDto
            {
                Id = f.Id,
                CustomerId = f.CustomerId,
                CustomerName = f.Customer!.Name,
                Name = f.Name,
                Location = f.Location
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<AssetDto>> GetAssetsAsync(AssetQueryParameters query, CancellationToken cancellationToken)
    {
        var assets = _db.Assets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            assets = assets.Where(a =>
                EF.Functions.Like(a.AssetCode, $"%{term}%") || EF.Functions.Like(a.Name, $"%{term}%"));
        }

        if (query.FacilityId.HasValue)
            assets = assets.Where(a => a.FacilityId == query.FacilityId.Value);

        if (query.Status.HasValue)
            assets = assets.Where(a => a.Status == query.Status.Value);

        assets = ApplySort(assets, query.SortBy, query.SortDescending);

        // Two indexed queries (count + page), same pattern as WorkOrders: required to stay
        // fast once the table holds 100k+ rows instead of loading everything into memory.
        var totalCount = await assets.LongCountAsync(cancellationToken);

        var items = await assets
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AssetDto
            {
                Id = a.Id,
                FacilityId = a.FacilityId,
                FacilityName = a.Facility!.Name,
                AssetCode = a.AssetCode,
                Name = a.Name,
                AssetType = a.AssetType,
                Status = a.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AssetDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Asset> ApplySort(IQueryable<Asset> query, string sortBy, bool descending)
    {
        // Whitelisted switch rather than a dynamic "OrderBy(sortBy)" string, same rationale as
        // WorkOrderService.ApplySort: keeps sorting safe from injection and every accepted
        // value maps to an indexed (or otherwise cheap) column.
        Expression<Func<Asset, object>> keySelector = sortBy.Trim().ToLowerInvariant() switch
        {
            "name" => a => a.Name,
            "status" => a => a.Status,
            "facilityid" => a => a.FacilityId,
            _ => a => a.AssetCode
        };

        return descending
            ? query.OrderByDescending(keySelector).ThenByDescending(a => a.Id)
            : query.OrderBy(keySelector).ThenBy(a => a.Id);
    }
}
