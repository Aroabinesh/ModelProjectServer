using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Lookups;
using ModelProject.Dtos.Technicians;
using ModelProject.Services.Technicians;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/technicians")]
[Produces("application/json")]
public class TechniciansController : ControllerBase
{
    private readonly ITechnicianService _technicianService;

    public TechniciansController(ITechnicianService technicianService)
    {
        _technicianService = technicianService;
    }

    /// <summary>List technicians. Backs the technician filter and the "Assign Technician" action.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TechnicianDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TechnicianDto>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        => Ok(await _technicianService.GetAllAsync(activeOnly, cancellationToken));

    /// <summary>Get a single technician by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TechnicianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicianDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _technicianService.GetByIdAsync(id, cancellationToken));

    /// <summary>Create a new technician.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TechnicianDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TechnicianDto>> Create(
        [FromBody] CreateTechnicianDto dto, CancellationToken cancellationToken)
    {
        var result = await _technicianService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a technician's name, email, active status, and employment dates.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TechnicianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicianDto>> Update(
        int id, [FromBody] UpdateTechnicianDto dto, CancellationToken cancellationToken)
    {
        var result = await _technicianService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a technician (only allowed when they have no assigned work orders).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _technicianService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
