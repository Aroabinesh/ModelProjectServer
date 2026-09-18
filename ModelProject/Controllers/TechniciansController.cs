using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Lookups;
using ModelProject.Services.Lookups;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/technicians")]
[Produces("application/json")]
public class TechniciansController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public TechniciansController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <summary>List technicians. Backs the technician filter and the "Assign Technician" action.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TechnicianDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TechnicianDto>>> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        => Ok(await _lookupService.GetTechniciansAsync(activeOnly, cancellationToken));
}
