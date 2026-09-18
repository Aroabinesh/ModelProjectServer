using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Lookups;
using ModelProject.Services.Lookups;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/assets")]
[Produces("application/json")]
public class AssetsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public AssetsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <summary>List assets, optionally filtered by facility. Backs the "asset" picker on the create/edit form.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll(
        [FromQuery] int? facilityId, CancellationToken cancellationToken)
        => Ok(await _lookupService.GetAssetsAsync(facilityId, cancellationToken));
}
