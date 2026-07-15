# Truckero.Diagnostics

This library houses all debugging, testing, and diagnostics tooling for the Truckero ecosystem.

## Areas

- Mock Services & Repositories
- Development-only utilities
- Seeder logic
- Performance/Monitoring Middleware
- Test Data Factories
- Simulators for testing workflows

## FreightBidSimulator

The `FreightBidSimulator` automatically generates simulated driver bids for freight requests during testing, allowing developers to test the FreightBidQueue workflow without requiring multiple real drivers.

### Key Features

- Automatically generates up to 4 driver bids with realistic data
- Adds new bids at timed intervals (10s for first bid, 6s for each subsequent bid)
- Exposes REST API endpoints for starting the simulator and retrieving bids
- Easy integration with ASP.NET Core applications

### Setup

To use the FreightBidSimulator in your project:

1. Register the simulator services in your application startup:

```csharp
// In Program.cs or Startup.cs
services.AddFreightBidSimulator();
```

2. Start the simulator via API:

```http
POST /api/diagnostics/freight-bid-simulator/start
```

3. Retrieve simulated bids:

```http
GET /api/diagnostics/freight-bid-simulator/bids
```

See the `Mocks/Services/FreightBidSimulator.md` file for detailed usage instructions.
