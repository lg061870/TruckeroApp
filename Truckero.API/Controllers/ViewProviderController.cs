using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.DTOs.Shared;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ViewProviderController : ControllerBase {
    private readonly ITruckService _truckService;
    private readonly IBankService _bankService;
    private readonly IPaymentMethodTypeService _paymentMethodTypeService;
    private readonly ICountryService _countryService;
    private readonly IHelpOptionService _helpOptionService;

    public ViewProviderController(
        ITruckService truckService,
        IBankService bankService,
        IPaymentMethodTypeService paymentMethodTypeService,
        ICountryService countryService,
        IHelpOptionService helpOptionService) {
        _truckService = truckService ?? throw new ArgumentNullException(nameof(truckService));
        _bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        _paymentMethodTypeService = paymentMethodTypeService ?? throw new ArgumentNullException(nameof(paymentMethodTypeService));
        _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        _helpOptionService = helpOptionService ?? throw new ArgumentNullException(nameof(helpOptionService)); 
    }

    [AllowAnonymous]
    [HttpGet("truck-page-data")]
    public async Task<ActionResult<TruckReferenceData>> GetTruckPageData() {
        var dto = new TruckReferenceData {
            TruckMakes = await _truckService.GetTruckMakesAsync(),
            TruckModels = await _truckService.GetTruckModelsAsync(),
            TruckCategories = await _truckService.GetTruckCategoriesAsync(),
            BedTypes = await _truckService.GetBedTypesAsync(),
            UseTags = await _truckService.GetUseTagsAsync(),
            TruckTypes = await _truckService.GetTruckTypesAsync()
        };
        return Ok(dto);
    }

    [AllowAnonymous]
    [HttpGet("payout-page-data")]
    public async Task<ActionResult<PayoutAccountReferenceDataRequest>> GetPayoutPageData([FromQuery] string? countryCode = "CR") {
        countryCode = (countryCode ?? "CR").Trim().ToUpperInvariant();

        var payoutMethodTypes = await _paymentMethodTypeService.GetPaymentMethodTypesByCountryAsync(countryCode)
            ?? new List<Core.DTOs.PaymentMethodType.PaymentMethodTypeRequest>();
        var banks = await _bankService.GetBanksByCountryCodeAsync(countryCode)
            ?? new List<Core.DTOs.Shared.BankRequest>();
        var countries = await _countryService.GetAllCountriesAsync()
            ?? new List<Core.DTOs.Shared.CountryRequest>();

        var dto = new PayoutAccountReferenceDataRequest {
            PayoutMethodTypes = payoutMethodTypes,
            Banks = banks,
            Countries = countries
        };

        return Ok(dto);
    }

    [HttpGet("payment-reference-data/country/{countryCode}")]
    public async Task<ActionResult<PaymentAccountReferenceData>> GetPaymentPageReferenceDataAsync(string countryCode) {
        countryCode = (countryCode ?? "CR").Trim().ToUpperInvariant();

        var referenceData = new PaymentAccountReferenceData {
            PaymentMethodTypes = await _paymentMethodTypeService.GetPaymentMethodTypesByCountryAsync(countryCode)
                                 ?? new List<PaymentMethodTypeRequest>(),
            Banks = await _bankService.GetBanksByCountryCodeAsync(countryCode)
                    ?? new List<BankRequest>(),
            Countries = await _countryService.GetAllCountriesAsync()
                       ?? new List<CountryRequest>()
        };
        return Ok(referenceData);
    }

   
    [AllowAnonymous]
    [HttpGet("freight-bid-reference-data")]
    public async Task<ActionResult<FreightBidReferenceData>> GetFreightBidReferenceData() {
        var truckTypes = await _truckService.GetTruckTypesAsync();
        var truckCategories = await _truckService.GetTruckCategoriesAsync();
        var bedTypes = await _truckService.GetBedTypesAsync();

        // If you have services for these; if not, use repository or static list.
        var payloadOptions = await _truckService.GetUseTagsAsync(); // Payload = UseTags per your mapping
        var useTags = await _truckService.GetUseTagsAsync();

        // For HelpOptions, implement similar to UseTag service/repository
        var helpOptions = await _helpOptionService.GetAllHelpOptionsAsync(); // create IHelpOptionService as needed

        var paymentMethodTypes = await _paymentMethodTypeService.GetAllPaymentMethodTypesAsync();

        var dto = new FreightBidReferenceData {
            TruckTypes = truckTypes?.ToList() ?? new(),
            TruckCategories = truckCategories?.ToList() ?? new(),
            BedTypes = bedTypes?.ToList() ?? new(),
            UseTags = useTags?.ToList() ?? new(),
            HelpOptions = helpOptions?.ToList() ?? new(),        // <-- MATCHED!
            PaymentMethodTypes = paymentMethodTypes?.ToList() ?? new()
        };


        return Ok(dto);
    }

}
