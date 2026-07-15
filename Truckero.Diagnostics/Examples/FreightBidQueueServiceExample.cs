using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Truckero.Diagnostics.Mocks.Services;

namespace Truckero.SampleUsage;

/// <summary>
/// Example of how to integrate the FreightBidSimulator into your application
/// This is a sample service that could be used by the FreightBidQueue.razor component
/// </summary>
public class FreightBidQueueService : IFreightBidQueueService
{
    private readonly HttpClient _httpClient;
    private readonly IEnvironmentService _environmentService;
    
    public FreightBidQueueService(HttpClient httpClient, IEnvironmentService environmentService)
    {
        _httpClient = httpClient;
        _environmentService = environmentService;
    }
    
    public async Task<GetBidsForJobResponse> GetBidsForJobAsync(GetBidsForJobRequest request)
    {
        // In development, use the simulator
        if (_environmentService.IsDevelopment())
        {
            // Option 1: Use the static method directly if in the same assembly
            var simulatedBids = FreightBidSimulator.GetSimulatedBids();
            
            // If no bids exist yet and this is a development environment,
            // we can automatically start the simulator
            if (!simulatedBids.Any() && request.FreightRequestId.HasValue)
            {
                // Start the simulator via HTTP request
                await _httpClient.PostAsJsonAsync(
                    "api/diagnostics/freight-bid-simulator/start", 
                    new { freightRequestId = request.FreightRequestId });
            }
            
            return new GetBidsForJobResponse
            {
                Success = true,
                Bids = simulatedBids.Where(b => 
                    !request.FreightRequestId.HasValue || 
                    request.FreightRequestId.Value == b.BidId
                ).ToList()
            };
        }
        
        // In production, call the real API
        var response = await _httpClient.GetFromJsonAsync<GetBidsForJobResponse>(
            $"api/bids?freightRequestId={request.FreightRequestId}");
            
        return response ?? new GetBidsForJobResponse { Success = false };
    }
}

public interface IFreightBidQueueService
{
    Task<GetBidsForJobResponse> GetBidsForJobAsync(GetBidsForJobRequest request);
}

public class GetBidsForJobRequest
{
    public Guid? FreightRequestId { get; set; }
}

public class GetBidsForJobResponse
{
    public bool Success { get; set; }
    public List<BidDetailsDto> Bids { get; set; } = new List<BidDetailsDto>();
}

public interface IEnvironmentService
{
    bool IsDevelopment();
}