using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentMethodsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all payment method types (e.g., Card, PayPal) supported by the platform.
    /// These are the options users can use to pay.
    /// </summary>
    [HttpGet("GetAllPaymentMethods")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? countryCode = null)
    {
        var methods = await _db.PaymentMethodTypes
            .Where(m => m.IsForPayment)
            .ToListAsync();

        return Ok(methods);
    }

    /// <summary>
    /// Returns all payout method types (e.g., Bank, PayPal) supported for drivers.
    /// </summary>
    [HttpGet("GetAllPayoutMethods")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPayoutMethods([FromQuery] string? countryCode = null)
    {
        var methods = await _db.PaymentMethodTypes
            .Where(m => m.IsForPayout)
            .ToListAsync();

        return Ok(methods);
    }
}
