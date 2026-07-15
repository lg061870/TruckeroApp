using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Truckero.API;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Store;
using Truckero.Infrastructure.Data;
using Xunit;

namespace Truckero.API.Tests;

public class StoreControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StoreControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("Truckero_StoreTests");
                });
            });
        });
    }

    private async Task<Guid> CreateCompanyAsync(HttpClient client)
    {
        var request = new CompanyRequest
        {
            Name = "Store Test Co",
            Industries = new List<string> { "Appliances" }
        };

        var response = await client.PostAsJsonAsync("/api/Company", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CompanyResponse>();
        var company = Assert.Single(payload!.CompanyRequests);
        return company.Id;
    }

    [Fact]
    public async Task Create_And_Get_Store_By_CompanyId_Works()
    {
        var client = _factory.CreateClient();

        var companyId = await CreateCompanyAsync(client);

        var storeRequest = new StoreRequest
        {
            CompanyId = companyId,
            Name = "Main Store",
            Address = "123 Test St",
            Region = "Central"
        };

        var createResponse = await client.PostAsJsonAsync("/api/Store", storeRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<StoreResponse>();
        Assert.NotNull(createPayload);
        Assert.True(createPayload!.Success);
        Assert.NotNull(createPayload.Store);
        var storeId = createPayload.Store!.Id;

        var listResponse = await client.GetAsync($"/api/Store/company/{companyId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<StoreListResponse>();
        Assert.NotNull(listPayload);
        Assert.True(listPayload!.Success);
        Assert.Contains(listPayload.Stores, s => s.Id == storeId && s.Name == "Main Store");
    }
}
