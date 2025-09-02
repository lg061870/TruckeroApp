using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients {
    public class CustomerApiClientService : ICustomerProfileService {
        private readonly HttpClient _http;
        private readonly IAuthSessionContext _session;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CustomerApiClientService(HttpClient http, IAuthSessionContext session) {
            _http = http;
            _session = session;
        }

        private string RequireAccessToken()
            => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

        /// <summary>
        /// Get customer profile (returns DTO, not entity)
        /// </summary>
        public async Task<CustomerProfileResponse> GetByUserByCustomerProfileIdAsync(Guid userId) {
            var envelope = AuthenticatedEnvelope.Create(
                RequireAccessToken(), _http, HttpMethod.Get, $"/api/Customer/{userId}"
            );
            var response = await envelope.SendAsync<HttpResponseMessage>();
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<CustomerProfileResponse>(json, _jsonOptions);
            if (dto == null)
                throw new Exception("Invalid response from API: CustomerProfileResponse was null.");

            return dto;
        }

        /// <summary>
        /// Create a customer profile (returns DTO, not entity)
        /// </summary>
        public async Task<CustomerProfileResponse> CreateCustomerProfileAsync(CustomerProfileRequest request, Guid userId) {
            var envelope = AuthenticatedEnvelope.Create(
                RequireAccessToken(), _http, HttpMethod.Post, $"/api/Customer?userId={userId}", request
            );
            var response = await envelope.SendAsync<HttpResponseMessage>();
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<CustomerProfileResponse>(json, _jsonOptions);
            if (dto == null)
                throw new Exception("Invalid response from API: CustomerProfileResponse was null.");

            return dto;
        }
    }
}
