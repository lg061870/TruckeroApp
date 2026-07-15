nonusing System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Truckero.API;
using Truckero.Core.DTOs;
using Truckero.Infrastructure.Data;
using Xunit;

namespace Truckero.API.Tests;

public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CompanyControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("Truckero_CompanyTests");
                });
            });
        });
    }

    [Fact]
    public async Task GetMetadata_Returns_BusinessTypes_And_Industries()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Company/metadata");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CompanyResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.NotNull(payload.BusinessTypes);
        Assert.NotNull(payload.IndustryTypes);
    }

    [Fact]
    public async Task Create_Then_GetById_RoundTrips_Company()
    {
        var client = _factory.CreateClient();

        var createRequest = new CompanyRequest
        {
            Name = "Test Company",
            LogoUrl = "https://example.com/logo.png",
            BusinessTypeId = null,
            Industries = new List<string> { "Furniture" }
        };

        var createResponse = await client.PostAsJsonAsync("/api/Company", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createPayload = await createResponse.Content.ReadFromJsonAsync<CompanyResponse>();
        Assert.NotNull(createPayload);
        Assert.True(createPayload!.Success);
        var created = Assert.Single(createPayload.CompanyRequests);
        Assert.NotEqual(Guid.Empty, created.Id);

        var getResponse = await client.GetAsync($"/api/Company/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getPayload = await getResponse.Content.ReadFromJsonAsync<CompanyResponse>();
        Assert.NotNull(getPayload);
        Assert.True(getPayload!.Success);
        var fetched = Assert.Single(getPayload.CompanyRequests);
        Assert.Equal("Test Company", fetched.Name);
    }
}
