using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.DTOs.Shared;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

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

        // -- REVISION STARTS HERE --
        var payoutMethodTypeResponse = await _paymentMethodTypeService.GetPaymentMethodTypesByCountryAsync(countryCode);
        var payoutMethodTypes = payoutMethodTypeResponse?.PaymentMethodTypes ?? new List<PaymentMethodTypeRequest>();

        var banks = await _bankService.GetBanksByCountryCodeAsync(countryCode)
            ?? new List<BankRequest>();
        var countries = await _countryService.GetAllCountriesAsync()
            ?? new List<CountryRequest>();

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

        // -- REVISION STARTS HERE --
        var paymentMethodTypeResponse = await _paymentMethodTypeService.GetPaymentMethodTypesByCountryAsync(countryCode);
        var paymentMethodTypes = paymentMethodTypeResponse?.PaymentMethodTypes ?? new List<PaymentMethodTypeRequest>();

        var banks = await _bankService.GetBanksByCountryCodeAsync(countryCode)
                ?? new List<BankRequest>();
        var countries = await _countryService.GetAllCountriesAsync()
                   ?? new List<CountryRequest>();

        var referenceData = new PaymentAccountReferenceData {
            PaymentMethodTypes = paymentMethodTypes,
            Banks = banks,
            Countries = countries
        };
        return Ok(referenceData);
    }

    [AllowAnonymous]
    [HttpGet("freight-bid-reference-data")]
    public async Task<ActionResult<FreightBidReferenceData>> GetFreightBidReferenceData() {
        // Truck data
        var truckTypes = await _truckService.GetTruckTypesAsync() ?? new List<TruckType>();
        var truckCategories = await _truckService.GetTruckCategoriesAsync() ?? new List<TruckCategory>();
        var bedTypes = await _truckService.GetBedTypesAsync() ?? new List<BedType>();
        var truckMakes = await _truckService.GetTruckMakesAsync() ?? new List<TruckMake>();
        var truckModels = await _truckService.GetTruckModelsAsync() ?? new List<TruckModel>();

        // Usage & help options
        var useTags = await _truckService.GetUseTagsAsync() ?? new List<UseTag>();
        var helpOptions = await _helpOptionService.GetAllHelpOptionsAsync() ?? new List<HelpOption>();

        // -- REVISION STARTS HERE --
        var paymentMethodTypeResponse = await _paymentMethodTypeService.GetAllPaymentMethodTypesAsync();
        var paymentMethodTypes = paymentMethodTypeResponse?.PaymentMethodTypes ?? new List<PaymentMethodTypeRequest>();

        var dto = new FreightBidReferenceData {
            TruckTypes = truckTypes,
            TruckCategories = truckCategories,
            BedTypes = bedTypes,
            TruckMakes = truckMakes,
            TruckModels = truckModels,
            UseTags = useTags,
            HelpOptions = helpOptions,
            PaymentMethodTypes = paymentMethodTypes
        };

        return Ok(dto);
    }
}
