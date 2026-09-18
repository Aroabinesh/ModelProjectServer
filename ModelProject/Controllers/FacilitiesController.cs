using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Lookups;
using ModelProject.Services.Lookups;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/facilities")]
[Produces("application/json")]
public class FacilitiesController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public FacilitiesController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <summary>List facilities, optionally filtered by customer. Backs the facility filter dropdown.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FacilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FacilityDto>>> GetAll(
        [FromQuery] int? customerId, CancellationToken cancellationToken)
        => Ok(await _lookupService.GetFacilitiesAsync(customerId, cancellationToken));
}
