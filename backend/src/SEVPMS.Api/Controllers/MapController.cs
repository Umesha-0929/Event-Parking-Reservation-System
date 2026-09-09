using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/maps")]
public sealed class MapController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [Authorize]
    [HttpGet("reverse")]
    public async Task<ActionResult<object>> Reverse([FromQuery] decimal lat, [FromQuery] decimal lon, CancellationToken cancellationToken)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180) return BadRequest(new { message = "Invalid map coordinates." });
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SEVPMS-Academic-Project/1.0");
        var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&addressdetails=1&lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return Ok(new { latitude = lat, longitude = lon });
            using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            var address = root.TryGetProperty("address", out var a) ? a : default;
            string? Pick(params string[] keys)
            {
                if (address.ValueKind != System.Text.Json.JsonValueKind.Object) return null;
                foreach (var key in keys) if (address.TryGetProperty(key, out var value) && value.ValueKind == System.Text.Json.JsonValueKind.String) return value.GetString();
                return null;
            }
            return Ok(new
            {
                latitude = lat,
                longitude = lon,
                displayName = root.TryGetProperty("display_name", out var display) ? display.GetString() : null,
                addressLine1 = Pick("road", "pedestrian", "neighbourhood", "suburb"),
                city = Pick("city", "town", "village", "municipality"),
                district = Pick("state_district", "county", "state"),
                country = Pick("country")
            });
        }
        catch
        {
            return Ok(new { latitude = lat, longitude = lon });
        }
    }
}
