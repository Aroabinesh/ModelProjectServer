using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Common;
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

    /// <summary>List assets with server-side search, filtering, sorting, and pagination. Backs the "asset" picker on the create/edit form.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetAll(
        [FromQuery] AssetQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _lookupService.GetAssetsAsync(query, cancellationToken));
}
