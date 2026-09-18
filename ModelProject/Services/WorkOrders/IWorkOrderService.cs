using ModelProject.Dtos.Common;
using ModelProject.Dtos.WorkOrders;

namespace ModelProject.Services.WorkOrders;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrderListItemDto>> GetListAsync(WorkOrderQueryParameters query, CancellationToken cancellationToken);
    Task<WorkOrderDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<WorkOrderDetailDto> CreateAsync(CreateWorkOrderDto dto, string createdBy, CancellationToken cancellationToken);
    Task<WorkOrderDetailDto> UpdateAsync(int id, UpdateWorkOrderDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<WorkOrderDetailDto> AssignTechnicianAsync(int id, AssignTechnicianDto dto, CancellationToken cancellationToken);
    Task<WorkOrderDetailDto> ChangeStatusAsync(int id, ChangeStatusDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<WorkOrderHistoryDto>> GetHistoryAsync(int id, CancellationToken cancellationToken);
}
