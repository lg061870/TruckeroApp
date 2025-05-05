using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Entities;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    [HttpGet("GetAllPaymentMethods")]
    [AllowAnonymous]
    public IActionResult GetAll(string countryCode)
    {
        var result = GetMethodList(countryCode);
        return Ok(result);
    }

    [HttpGet("GetAllPayoutMethods")]
    [AllowAnonymous]
    public IActionResult GetAllPayoutMethods(string countryCode)
    {
        var result = GetPayoutMethodList(countryCode);
        return Ok(result);
    }

    private static List<PaymentMethodType> GetMethodList(string countryCode)
    {
        var result = new List<PaymentMethodType>
        {
            new() { Id = Guid.NewGuid(), Name = "CreditCard", Description = "Visa, Mastercard, Amex" },
            new() { Id = Guid.NewGuid(), Name = "PayPal", Description = "Connect your PayPal account" },
            new() { Id = Guid.NewGuid(), Name = "GooglePay", Description = "Use Google Wallet" },
            new() { Id = Guid.NewGuid(), Name = "ApplePay", Description = "Use Apple Wallet" }
        };

        if (countryCode.ToUpper() == "CRI")
        {
            result.Add(new PaymentMethodType
            {
                Id = Guid.NewGuid(),
                Name = "SINPE",
                Description = "Costa Rica bank transfer via SINPE"
            });
        }

        return result;
    }

    private static List<PaymentMethodType> GetPayoutMethodList(string countryCode)
    {
        var result = new List<PaymentMethodType>
        {
            new() { Id = Guid.NewGuid(), Name = "BankAccount", Description = "Standard bank transfer" },
            new() { Id = Guid.NewGuid(), Name = "PayPal", Description = "Payout to your PayPal account" }
        };

        if (countryCode.ToUpper() == "CRI")
        {
            result.Add(new PaymentMethodType
            {
                Id = Guid.NewGuid(),
                Name = "SINPE",
                Description = "Costa Rica payout via SINPE"
            });
        }

        return result;
    }
}
