# FreightBidSimulator

This diagnostic tool helps simulate driver bids for freight requests during development and testing.

## Purpose

The `FreightBidSimulator` automatically generates simulated driver bids for a freight request, allowing developers to test the FreightBidQueue functionality without requiring multiple real drivers to respond to freight requests.

## Features

- Automatically simulates up to 4 driver bids for the most recent freight request
- Creates bids with realistic data (prices, arrival times, driver ratings, etc.)
- Each bid appears after a delay to simulate real-world behavior
- API endpoints for starting the simulator and retrieving simulated bids

## Setup

1. Register the simulator services in your application startup:

```csharp
// In Program.cs or Startup.cs
using Truckero.Diagnostics.Mocks.Services;

// ...

// Add this line to your service configuration
services.AddFreightBidSimulator();
```

## Usage

### Starting the Simulator via API

To generate bids for the most recent freight request:

```http
POST /api/diagnostics/freight-bid-simulator/start
```

To generate bids for a specific freight request:

```http
POST /api/diagnostics/freight-bid-simulator/start
Content-Type: application/json

{
  "freightRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Retrieving Simulated Bids

Get all simulated bids:

```http
GET /api/diagnostics/freight-bid-simulator/bids
```

Get bids for a specific freight request:

```http
GET /api/diagnostics/freight-bid-simulator/bids?freightRequestId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Clearing Simulated Bids

```http
DELETE /api/diagnostics/freight-bid-simulator/bids
```

## How it Works

1. When started, the simulator waits 10 seconds, then creates the first driver bid
2. It then waits 6 seconds before creating each additional bid (up to a total of 4 bids)
3. Each bid contains realistic driver and vehicle information
4. Simulated bids appear in the FreightBidQueue component just like real bids would

## Example Integration with FreightBidQueue

You can modify your `IFreightBidQueueService` implementation to use the simulated bids during development:

```csharp
public async Task<GetBidsForJobResponse> GetBidsForJobAsync(GetBidsForJobRequest request)
{
    // In development, use simulated bids
    if (_environment.IsDevelopment())
    {
        var simulatedBids = FreightBidSimulator.GetSimulatedBids();
        return new GetBidsForJobResponse
        {
            Success = true,
            Bids = simulatedBids.Select(b => new BidDetailsDto
            {
                BidId = b.BidId,
                DriverId = b.DriverId,
                // ... map other properties
            }).ToList()
        };
    }
    
    // In production, use real bids from the API
    // ...
}
```