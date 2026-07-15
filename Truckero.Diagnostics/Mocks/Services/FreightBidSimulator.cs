using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Timers;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;

namespace Truckero.Diagnostics.Mocks.Services;

/// <summary>
/// Simulates driver bid responses for testing the FreightBidQueue functionality.
/// This service automatically generates up to 4 bids for the most recent FreightBidRequest.
/// </summary>
public class FreightBidSimulator : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FreightBidSimulator> _logger;
    private System.Timers.Timer? _timer;
    private int _bidCount = 0;
    private Guid _currentFreightRequestId;
    private bool _isRunning = false;
    private readonly Random _random = new Random();
    
    // Vehicle types for simulation
    private readonly string[] _vehicleTypes = new[] { "Box Truck", "Pickup Truck", "Flatbed Truck", "Van" };
    
    // Categories for simulation
    private readonly string[] _categories = new[] { "Express", "Standard", "Oversized", "Fragile" };
    
    // Sizes for simulation
    private readonly string[] _sizes = new[] { "Small", "Medium", "Large", "Extra Large" };
    
    // Help options for simulation
    private readonly string[] _helpOptions = new[] { "Loading Only", "Loading & Unloading", "No Help", "Full Service" };

    public FreightBidSimulator(IServiceProvider serviceProvider, ILogger<FreightBidSimulator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the simulator to create bids for the most recent FreightRequest.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogInformation("Simulator already running");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Get the latest freight request (adjust this to match your actual entity name)
            var latestFreightRequest = dbContext.Set<FreightRequest>()
                .OrderByDescending(fr => fr.CreatedAt)
                .FirstOrDefault();

            if (latestFreightRequest == null)
            {
                _logger.LogWarning("No FreightRequest found to simulate bids for");
                return;
            }

            _currentFreightRequestId = latestFreightRequest.Id;
            _logger.LogInformation($"Starting bid simulator for FreightRequest {_currentFreightRequestId}");
            
            // Wait 10 seconds before the first bid
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
            _timer.Start();
            _isRunning = true;
        }
    }
    
    /// <summary>
    /// Manually start the simulator with a specific freight request ID.
    /// </summary>
    /// <param name="freightRequestId">The ID of the freight request to generate bids for</param>
    public void Start(Guid freightRequestId)
    {
        if (_isRunning)
        {
            _logger.LogInformation("Simulator already running");
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Check if the specified freight request exists
            var freightRequest = dbContext.Set<FreightRequest>().Find(freightRequestId);
            
            if (freightRequest == null)
            {
                _logger.LogWarning($"FreightRequest {freightRequestId} not found");
                return;
            }

            _currentFreightRequestId = freightRequestId;
            _logger.LogInformation($"Starting bid simulator for FreightRequest {_currentFreightRequestId}");
            
            // Wait 10 seconds before the first bid
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
            _timer.Start();
            _isRunning = true;
        }
    }

    /// <summary>
    /// Stops the simulator if it's running.
    /// </summary>
    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        _isRunning = false;
        _bidCount = 0;
        _logger.LogInformation("Bid simulator stopped");
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            CreateNewBid();
            _bidCount++;

            if (_bidCount < 4)
            {
                // Set timer for 6 seconds for the next bid
                _timer!.Interval = 6000;
                _timer.Start();
            }
            else
            {
                _logger.LogInformation("Finished creating 4 bids");
                Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating simulated bid");
            Stop();
        }
    }

    private void CreateNewBid()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Check if the freight request still exists
        var freightRequest = dbContext.Set<FreightRequest>().Find(_currentFreightRequestId);
        if (freightRequest == null)
        {
            _logger.LogWarning($"FreightRequest {_currentFreightRequestId} no longer exists");
            Stop();
            return;
        }

        // Get available drivers to simulate bids from
        var drivers = dbContext.Set<DriverProfile>().Include(d => d.User).Take(10).ToList();
        if (!drivers.Any())
        {
            _logger.LogWarning("No drivers available to simulate bids");
            Stop();
            return;
        }

        // Choose a random driver
        var driver = drivers[_random.Next(drivers.Count)];

        // Check if this driver already bid on this job
        var existingBid = dbContext.Set<DriverBid>()
            .FirstOrDefault(b => b.FreightRequestId == _currentFreightRequestId && b.DriverProfileId == driver.Id);
        
        if (existingBid != null)
        {
            _logger.LogInformation($"Driver {driver.Id} already bid on this job");
            // Try to create the bid anyway, just to make sure we get 4 bids
        }

        // Generate a random price between $50 and $150
        decimal price = _random.Next(5000, 15000) / 100m;
        
        // Generate random values for simulation
        string vehicleType = _vehicleTypes[_random.Next(_vehicleTypes.Length)];
        string category = _categories[_random.Next(_categories.Length)];
        string size = _sizes[_random.Next(_sizes.Length)];
        string help = _helpOptions[_random.Next(_helpOptions.Length)];
        double distanceMiles = Math.Round(_random.NextDouble() * 19 + 1, 1); // 1.0 to 20.0 miles
        
        // Create a simulated bid in the database
        var newBid = new DriverBid
        {
            Id = Guid.NewGuid(),
            FreightRequestId = _currentFreightRequestId,
            DriverProfileId = driver.Id,
            Price = price,
            EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(_random.Next(10, 60)),
            Status = "Available", // Adjust based on your actual enum or string values
            Comment = GetRandomComment(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Set<DriverBid>().Add(newBid);
        dbContext.SaveChanges();

        // Also create a corresponding BidDetailsDto to expose through an API endpoint or similar
        // for the FreightBidQueue component
        var bidDetails = new BidDetailsDto
        {
            BidId = newBid.Id,
            DriverId = driver.Id,
            DriverName = driver.User?.FullName ?? "Unknown Driver",
            DriverAvatarUrl = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(driver.User?.FullName ?? "Unknown"),
            PriceTotal = price,
            PriceBase = price * 0.8m,
            ServiceFee = price * 0.2m,
            Tax = 0,
            Rating = Math.Round(_random.NextDouble() * 2 + 3, 1), // 3.0 to 5.0 rating
            RatingCount = _random.Next(5, 100), // 5 to 100 ratings
            DistanceMiles = distanceMiles,
            EstimatedArrivalMinutes = _random.Next(10, 60),
            VehicleType = vehicleType,
            Category = category,
            Size = size,
            Help = help,
            Comment = newBid.Comment,
            CreatedAt = newBid.CreatedAt
        };

        // Store the bid details somewhere accessible, such as a static collection or database
        StoreBidDetails(bidDetails);

        _logger.LogInformation($"Created bid #{_bidCount + 1} from driver {driver.Id} with price {price:C}");
    }
    
    // Static collection to store bid details for retrieval
    private static readonly List<BidDetailsDto> _simulatedBids = new List<BidDetailsDto>();
    
    private void StoreBidDetails(BidDetailsDto bidDetails)
    {
        lock (_simulatedBids)
        {
            _simulatedBids.Add(bidDetails);
        }
    }
    
    /// <summary>
    /// Gets all simulated bids generated by this simulator
    /// </summary>
    public static IReadOnlyList<BidDetailsDto> GetSimulatedBids()
    {
        lock (_simulatedBids)
        {
            return _simulatedBids.ToList().AsReadOnly();
        }
    }
    
    /// <summary>
    /// Gets simulated bids for a specific freight request
    /// </summary>
    public static IReadOnlyList<BidDetailsDto> GetSimulatedBidsForRequest(Guid freightRequestId)
    {
        lock (_simulatedBids)
        {
            return _simulatedBids.Where(b => b.BidId == freightRequestId).ToList().AsReadOnly();
        }
    }
    
    /// <summary>
    /// Clears all simulated bids
    /// </summary>
    public static void ClearSimulatedBids()
    {
        lock (_simulatedBids)
        {
            _simulatedBids.Clear();
        }
    }

    private string GetRandomComment()
    {
        string[] comments = new[]
        {
            "I can help you with this delivery!",
            "I'm nearby and available to pick up your items.",
            "I have experience with this type of freight.",
            "I can get there quickly.",
            "My truck is perfect for this job.",
            "I've done many deliveries like this before.",
            "I'm just finishing a delivery nearby.",
            "I can offer fast service for this job.",
            "I have the right equipment for this load.",
            "Available now and can start immediately!"
        };

        return comments[_random.Next(comments.Length)];
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}