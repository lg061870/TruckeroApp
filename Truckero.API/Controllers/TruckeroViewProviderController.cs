using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Explicitly allow unauthenticated access
public class TruckeroViewProviderController : ControllerBase
{
    private readonly ITruckService _truckService;

    public TruckeroViewProviderController(ITruckService truckService)
    {
        _truckService = truckService;
    }

    [AllowAnonymous]
    [HttpGet("truck-page-data")]
    public async Task<ActionResult<TruckReferenceData>> GetTruckPageData()
    {
        var dto = new TruckReferenceData
        {
            TruckMakes = await _truckService.GetTruckMakesAsync(),
            TruckModels = await _truckService.GetTruckModelsAsync(),
            TruckCategories = await _truckService.GetTruckCategoriesAsync(),
            BedTypes = await _truckService.GetBedTypesAsync(),
            UseTags = await _truckService.GetUseTagsAsync(),
            TruckTypes = await _truckService.GetTruckTypesAsync()
        };
        return Ok(dto);
    }
}
