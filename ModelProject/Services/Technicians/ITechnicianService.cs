using ModelProject.Dtos.Lookups;
using ModelProject.Dtos.Technicians;

namespace ModelProject.Services.Technicians;

public interface ITechnicianService
{
    Task<IReadOnlyList<TechnicianDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<TechnicianDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TechnicianDto> CreateAsync(CreateTechnicianDto dto, CancellationToken cancellationToken);
    Task<TechnicianDto> UpdateAsync(int id, UpdateTechnicianDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
