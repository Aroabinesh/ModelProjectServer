using Microsoft.AspNetCore.Mvc;
using ModelProject.Dtos.Lookups;
using ModelProject.Services.Lookups;

namespace ModelProject.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public CustomersController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <summary>List all customers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _lookupService.GetCustomersAsync(cancellationToken));
}
