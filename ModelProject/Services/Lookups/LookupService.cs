using Microsoft.EntityFrameworkCore;
using ModelProject.Data;
using ModelProject.Dtos.Lookups;

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

    public async Task<IReadOnlyList<AssetDto>> GetAssetsAsync(int? facilityId, CancellationToken cancellationToken)
    {
        var assets = _db.Assets.AsNoTracking().AsQueryable();

        if (facilityId.HasValue)
            assets = assets.Where(a => a.FacilityId == facilityId.Value);

        return await assets
            .OrderBy(a => a.AssetCode)
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
    }

    public async Task<IReadOnlyList<TechnicianDto>> GetTechniciansAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var technicians = _db.Technicians.AsNoTracking().AsQueryable();

        if (activeOnly)
            technicians = technicians.Where(t => t.IsActive);

        return await technicians
            .OrderBy(t => t.Name)
            .Select(t => new TechnicianDto { Id = t.Id, Name = t.Name, Email = t.Email, IsActive = t.IsActive })
            .ToListAsync(cancellationToken);
    }
}
