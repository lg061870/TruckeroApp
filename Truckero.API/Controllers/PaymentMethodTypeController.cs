using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.PaymentMethodType;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodTypeController : ControllerBase {
    private readonly IPaymentMethodTypeService _service;

    public PaymentMethodTypeController(IPaymentMethodTypeService service) {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodTypeResponse>> GetAll() {
        var types = await _service.GetAllPaymentMethodTypesAsync();
        return Ok(new PaymentMethodTypeResponse {
            Success = true,
            Message = "Fetched all payment method types.",
            PaymentMethodTypes = types
        });
    }

    [HttpGet("country/{countryCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodTypeResponse>> GetByCountry(string countryCode) {
        var types = await _service.GetPaymentMethodTypesByCountryAsync(countryCode);
        return Ok(new PaymentMethodTypeResponse {
            Success = true,
            Message = $"Fetched payment method types for {countryCode}.",
            PaymentMethodTypes = types
        });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodTypeResponse>> GetById(Guid id) {
        var dto = await _service.GetPaymentMethodTypeByIdAsync(id);
        if (dto == null)
            return NotFound(new PaymentMethodTypeResponse {
                Success = false,
                Message = "Payment method type not found.",
                ErrorCode = "NOT_FOUND"
            });

        return Ok(new PaymentMethodTypeResponse {
            Success = true,
            Message = "Fetched payment method type.",
            PaymentMethodTypes = new List<PaymentMethodTypeRequest> { dto }
        });
    }

    // --- Admin endpoints follow same pattern, always wrap in PaymentMethodTypeResponse as needed ---
}
