using ModelProject.Dtos.Lookups;

namespace ModelProject.Services.Lookups;

public interface ILookupService
{
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FacilityDto>> GetFacilitiesAsync(int? customerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetDto>> GetAssetsAsync(int? facilityId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TechnicianDto>> GetTechniciansAsync(bool activeOnly, CancellationToken cancellationToken);
}
