using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

public class CustomerApiClientService : ICustomerService
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CustomerApiClientService(HttpClient http, IAuthSessionContext session)
    {
        _http = http;
        _session = session;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task<CustomerProfile?> GetByUserIdAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Get, $"/api/Customer/{userId}"
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerProfile>(json, _jsonOptions);
    }

    public async Task CreateAsync(CustomerProfileRequest request, Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Post, $"/api/Customer?userId={userId}", request
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }
}
