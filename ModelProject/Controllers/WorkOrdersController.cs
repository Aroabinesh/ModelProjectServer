using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Common;
using ModelProject.Dtos.WorkOrders;
using ModelProject.Services.WorkOrders;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/workorders")]
[Produces("application/json")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    /// <summary>List work orders with server-side search, filtering, sorting, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WorkOrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WorkOrderListItemDto>>> GetList(
        [FromQuery] WorkOrderQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.GetListAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get full details for a single work order.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new work order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderDetailDto>> Create(
        [FromBody] CreateWorkOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.CreateAsync(dto, dto.CreatedBy ?? "unknown", cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a work order's title, description, and priority.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkOrderDetailDto>> Update(
        int id, [FromBody] UpdateWorkOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a work order.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _workOrderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Assign (or reassign) a technician to a work order.</summary>
    [HttpPut("{id:int}/assign")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkOrderDetailDto>> AssignTechnician(
        int id, [FromBody] AssignTechnicianDto dto, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.AssignTechnicianAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>Change a work order's status, enforcing New -> Assigned -> InProgress -> Completed.</summary>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkOrderDetailDto>> ChangeStatus(
        int id, [FromBody] ChangeStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.ChangeStatusAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get the full status-change history for a work order.</summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkOrderHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkOrderHistoryDto>>> GetHistory(int id, CancellationToken cancellationToken)
    {
        var result = await _workOrderService.GetHistoryAsync(id, cancellationToken);
        return Ok(result);
    }
}
