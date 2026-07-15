using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Truckero.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/map/tiles")]
public class MapTilesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MapTilesController> _logger;

    public MapTilesController(IHttpClientFactory httpClientFactory, ILogger<MapTilesController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("{z:int}/{x:int}/{y:int}.png")]
    public async Task<IActionResult> GetTile(int z, int x, int y, CancellationToken cancellationToken)
    {
        if (z < 0 || z > 20 || x < 0 || y < 0)
        {
            return BadRequest("Invalid tile coordinates.");
        }

        var tileUrl = $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        try
        {
            var client = _httpClientFactory.CreateClient("osm-tiles");
            using var response = await client.GetAsync(tileUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var status = $"{(int)response.StatusCode} {response.ReasonPhrase}";
                _logger.LogWarning("Tile upstream failed for z={Z},x={X},y={Y}: {Status}", z, x, y, status);
                return StatusCode((int)response.StatusCode, status);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Response.Headers.CacheControl = "public,max-age=300";
            return File(bytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tile proxy failed for z={Z},x={X},y={Y}", z, x, y);
            return StatusCode(502, "Tile proxy failure.");
        }
    }
}
