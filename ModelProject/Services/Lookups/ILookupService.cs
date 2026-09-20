using ModelProject.Dtos.Common;
using ModelProject.Dtos.Lookups;

namespace ModelProject.Services.Lookups;

public interface ILookupService
{
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FacilityDto>> GetFacilitiesAsync(int? customerId, CancellationToken cancellationToken);
    Task<PagedResult<AssetDto>> GetAssetsAsync(AssetQueryParameters query, CancellationToken cancellationToken);
}
