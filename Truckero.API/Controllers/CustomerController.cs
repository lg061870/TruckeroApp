using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _repo;

    public CustomerController(ICustomerRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<CustomerProfile>> GetByUserId(Guid userId)
    {
        var profile = await _repo.GetByUserIdAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerProfile profile)
    {
        await _repo.AddAsync(profile);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByUserId), new { userId = profile.UserId }, profile);
    }
}
