using System.Net.Http.Json;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.PaymentAccount;  // <-- Add this
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class ViewProviderApiClientService : IViewProviderApiClientService {
    private readonly HttpClient _httpClient;

    public ViewProviderApiClientService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<PayoutAccountReferenceDataRequest?> GetPayoutPageReferenceDataAsync(string? countryCode = "CR") {
        var url = $"api/viewprovider/payout-page-data?countryCode={countryCode}";
        return await _httpClient.GetFromJsonAsync<PayoutAccountReferenceDataRequest>(url);
    }

    public async Task<PaymentAccountReferenceData?> GetPaymentPageReferenceDataAsync(string? countryCode = "CR") {
        // Matches the controller route: [HttpGet("payment-reference-data/country/{countryCode}")]
        var url = $"api/viewprovider/payment-reference-data/country/{countryCode}";
        return await _httpClient.GetFromJsonAsync<PaymentAccountReferenceData>(url);
    }

    public async Task<FreightBidReferenceData?> GetFreightBidReferenceDataAsync() {
        var url = "api/viewprovider/freight-bid-reference-data";
        return await _httpClient.GetFromJsonAsync<FreightBidReferenceData>(url);
    }
}
