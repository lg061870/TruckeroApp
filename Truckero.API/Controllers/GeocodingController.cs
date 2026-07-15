using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Truckero.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/geocoding")]
[Produces(MediaTypeNames.Application.Json)]
public class GeocodingController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeocodingController> _logger;

    public GeocodingController(IHttpClientFactory httpClientFactory, ILogger<GeocodingController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("reverse")]
    public async Task<IActionResult> Reverse([FromQuery] double lat, [FromQuery] double lon, CancellationToken cancellationToken)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180)
        {
            return BadRequest(new { error = "Invalid coordinates." });
        }

        var latText = lat.ToString("0.######", CultureInfo.InvariantCulture);
        var lonText = lon.ToString("0.######", CultureInfo.InvariantCulture);
        var requestUrl = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latText}&lon={lonText}&accept-language=en";

        try
        {
            var client = _httpClientFactory.CreateClient("nominatim");
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            using var response = await client.SendAsync(request, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusText = $"{(int)response.StatusCode} {response.ReasonPhrase}";

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Reverse geocoding failed. Status: {Status}. Body: {Body}", statusText, body);
                return StatusCode((int)response.StatusCode, new
                {
                    success = false,
                    status = statusText,
                    message = Trim(body),
                    requestUrl
                });
            }

            using var jsonDoc = JsonDocument.Parse(body);
            var address = jsonDoc.RootElement.TryGetProperty("display_name", out var displayName)
                ? displayName.GetString()
                : null;

            return Ok(new
            {
                success = !string.IsNullOrWhiteSpace(address),
                status = statusText,
                message = string.IsNullOrWhiteSpace(address) ? "display_name missing in response." : string.Empty,
                requestUrl,
                address
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reverse geocoding request failed for lat={Lat}, lon={Lon}", lat, lon);
            return StatusCode(502, new
            {
                success = false,
                status = "502 Bad Gateway",
                message = ex.Message,
                requestUrl
            });
        }
    }

    private static string Trim(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "No response body.";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 220 ? trimmed : $"{trimmed[..220]}...";
    }
}
