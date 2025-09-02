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
        var response = await _service.GetAllPaymentMethodTypesAsync();
        response.Success = true;
        response.Message = "Fetched all payment method types.";
        return Ok(response);
    }

    [HttpGet("country/{countryCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodTypeResponse>> GetByCountry(string countryCode) {
        var response = await _service.GetPaymentMethodTypesByCountryAsync(countryCode);
        response.Success = true;
        response.Message = $"Fetched payment method types for {countryCode}.";
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodTypeResponse>> GetById(Guid id) {
        var response = await _service.GetPaymentMethodTypeByIdAsync(id);

        if (response == null || response.PaymentMethodTypes == null || !response.PaymentMethodTypes.Any())
            return NotFound(new PaymentMethodTypeResponse {
                Success = false,
                Message = "Payment method type not found.",
                ErrorCode = "NOT_FOUND"
            });

        response.Success = true;
        response.Message = "Fetched payment method type.";
        return Ok(response);
    }

    // --- Admin endpoints follow same pattern, always wrap in PaymentMethodTypeResponse as needed ---
}
