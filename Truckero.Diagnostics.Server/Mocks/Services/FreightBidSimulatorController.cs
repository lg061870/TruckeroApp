using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Truckero.Diagnostics.Mocks.Services
{
    /// <summary>
    /// API Controller for simulating freight bid responses
    /// Exposes endpoints to start/stop the simulator and retrieve simulated bids
    /// </summary>
    [Route("api/diagnostics/freight-bid-simulator")]
    [ApiController]
    public class FreightBidSimulatorController : ControllerBase
    {
        private readonly ILogger<FreightBidSimulatorController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FreightBidSimulatorController(
            ILogger<FreightBidSimulatorController> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("start")]
        public IActionResult StartSimulator([FromBody] StartSimulatorRequest request)
        {
            try
            {
                // Create a new simulator instance and use a factory to get the correct logger type
                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                var simulatorLogger = loggerFactory.CreateLogger<FreightBidSimulator>();
                var simulator = new FreightBidSimulator(_serviceProvider, simulatorLogger);

                if (request?.FreightRequestId != null && request.FreightRequestId != Guid.Empty)
                {
                    // Start for specific request
                    simulator.Start(request.FreightRequestId.Value);
                    return Ok(new { message = $"Simulator started for freight request {request.FreightRequestId}" });
                }
                else
                {
                    // Start for most recent request
                    simulator.Start();
                    return Ok(new { message = "Simulator started for most recent freight request" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting bid simulator");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("bids")]
        public IActionResult GetSimulatedBids([FromQuery] Guid? freightRequestId = null)
        {
            try
            {
                if (freightRequestId.HasValue)
                {
                    var bids = FreightBidSimulator.GetSimulatedBidsForRequest(freightRequestId.Value);
                    return Ok(bids);
                }
                else
                {
                    var bids = FreightBidSimulator.GetSimulatedBids();
                    return Ok(bids);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving simulated bids");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpDelete("bids")]
        public IActionResult ClearSimulatedBids()
        {
            try
            {
                FreightBidSimulator.ClearSimulatedBids();
                return Ok(new { message = "All simulated bids cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing simulated bids");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for starting the simulator
    /// </summary>
    public class StartSimulatorRequest
    {
        public Guid? FreightRequestId { get; set; }
    }
}