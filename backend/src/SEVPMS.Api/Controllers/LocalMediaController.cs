using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Api.Controllers;

[ApiController]
public sealed class LocalMediaController(
    IWebHostEnvironment environment,
    IEventRepository eventRepository) : ControllerBase
{
    private const long MaxGenericBytes = 15_000_000;
    private const long MaxImageBytes = 10_000_000;
    private const int MaxGenericFilesPerUserCategory = 50;

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".mp4", ".webm", ".mov", ".m4v" };

    [Authorize]
    [EnableRateLimiting("sensitive")]
    [RequestSizeLimit(MaxGenericBytes)]
    [HttpPost("api/media/uploads")]
    public async Task<ActionResult<object>> Upload(
        IFormFile file,
        [FromQuery] string category = "general",
        CancellationToken cancellationToken = default)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var validation = await ValidateAsync(file, allowVideo: true, MaxGenericBytes, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        var safeCategory = SafeSegment(category);
        var dir = Path.Combine(Root, safeCategory);
        Directory.CreateDirectory(dir);
        var prefix = $"{userId:N}-";
        if (Directory.EnumerateFiles(dir, prefix + "*").Take(MaxGenericFilesPerUserCategory).Count() >= MaxGenericFilesPerUserCategory)
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Media quota reached for this category. Remove old media before uploading more." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{prefix}{Guid.NewGuid():N}{ext}";
        var path = MediaPath(safeCategory, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new
        {
            url = $"/api/media/files/{safeCategory}/{fileName}",
            type = ImageExtensions.Contains(ext) ? "Photo" : "Video",
            fileName
        });
    }

    [AllowAnonymous]
    [HttpGet("api/media/files/{category}/{fileName}")]
    public IActionResult FileByName(string category, string fileName)
    {
        var safeCategory = SafeSegment(category);
        var safeFile = Path.GetFileName(fileName);
        var path = MediaPath(safeCategory, safeFile);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, ContentType(Path.GetExtension(path)), enableRangeProcessing: true);
    }

    [AllowAnonymous]
    [HttpGet("api/events/{eventId:guid}/cover")]
    public IActionResult GetEventCover(Guid eventId)
    {
        var item = FindByPrefix("events", $"event-{eventId:N}-cover");
        return item is null ? NotFound() : Ok(new { url = item });
    }

    [Authorize(Policy = AuthorizationPolicies.EventOrganizerOnly)]
    [EnableRateLimiting("sensitive")]
    [RequestSizeLimit(MaxImageBytes)]
    [HttpPost("api/events/{eventId:guid}/cover")]
    public async Task<ActionResult<object>> SetEventCover(
        Guid eventId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity is null) return NotFound(new { message = "Event not found." });
        if (eventEntity.OrganizerUserId != userId) return Forbid();
        var validation = await ValidateAsync(file, allowVideo: false, MaxImageBytes, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        DeleteByPrefix("events", $"event-{eventId:N}-cover");
        var fileName = $"event-{eventId:N}-cover{ext}";
        var path = MediaPath("events", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return Ok(new { url = $"/api/media/files/events/{fileName}" });
    }

    [Authorize(Roles = "EventOrganizer,VenueOwner")]
    [EnableRateLimiting("sensitive")]
    [RequestSizeLimit(MaxImageBytes)]
    [HttpPost("api/payment-qr/mine")]
    public async Task<ActionResult<object>> SetPaymentQr(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        var validation = await ValidateAsync(file, allowVideo: false, MaxImageBytes, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        DeleteByPrefix("payment-qrs", $"payment-{userId:N}");
        var fileName = $"payment-{userId:N}{ext}";
        var path = MediaPath("payment-qrs", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return Ok(new { userId, url = $"/api/media/files/payment-qrs/{fileName}" });
    }

    [Authorize]
    [HttpGet("api/payment-qr/mine")]
    public IActionResult GetMyPaymentQr()
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        return GetPaymentQr(userId);
    }

    [Authorize]
    [HttpGet("api/payment-qr/users/{userId:guid}")]
    public IActionResult GetPaymentQr(Guid userId)
    {
        var item = FindByPrefix("payment-qrs", $"payment-{userId:N}");
        return item is null ? NotFound(new { message = "Payment QR has not been added yet." }) : Ok(new { userId, url = item });
    }

    private string Root => Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
    private string MediaPath(string category, string fileName) => Path.Combine(Root, category, fileName);

    private static string SafeSegment(string value)
    {
        var cleaned = new string((value ?? "general").Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_').Take(48).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "general" : cleaned.ToLowerInvariant();
    }

    private async Task<string?> ValidateAsync(IFormFile? file, bool allowVideo, long maxBytes, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return "Choose a media file first.";
        if (file.Length > maxBytes) return $"Media files must be {maxBytes / 1_000_000} MB or smaller.";
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExtensions.Contains(ext) && !(allowVideo && VideoExtensions.Contains(ext)))
            return allowVideo ? "Supported files: JPG, PNG, WebP, GIF, MP4, WebM, MOV." : "Only JPG, PNG, WebP or GIF images are accepted.";

        await using var stream = file.OpenReadStream();
        var header = new byte[16];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), ct);
        if (read < 4 || !SignatureMatches(ext, header.AsSpan(0, read)))
            return "The file contents do not match the selected media type.";
        return null;
    }

    private static bool SignatureMatches(string ext, ReadOnlySpan<byte> h) => ext switch
    {
        ".jpg" or ".jpeg" => h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,
        ".png" => h.Length >= 8 && h[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        ".gif" => h.Length >= 6 && (System.Text.Encoding.ASCII.GetString(h[..6]) is "GIF87a" or "GIF89a"),
        ".webp" => h.Length >= 12 && System.Text.Encoding.ASCII.GetString(h[..4]) == "RIFF" && System.Text.Encoding.ASCII.GetString(h.Slice(8, 4)) == "WEBP",
        ".webm" => h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3,
        ".mp4" or ".mov" or ".m4v" => h.Length >= 12 && System.Text.Encoding.ASCII.GetString(h.Slice(4, 4)) == "ftyp",
        _ => false
    };

    private string? FindByPrefix(string category, string prefix)
    {
        var dir = Path.Combine(Root, category);
        if (!Directory.Exists(dir)) return null;
        var path = Directory.EnumerateFiles(dir, prefix + ".*").OrderByDescending(System.IO.File.GetLastWriteTimeUtc).FirstOrDefault();
        return path is null ? null : $"/api/media/files/{category}/{Path.GetFileName(path)}";
    }

    private void DeleteByPrefix(string category, string prefix)
    {
        var dir = Path.Combine(Root, category);
        if (!Directory.Exists(dir)) return;
        foreach (var path in Directory.EnumerateFiles(dir, prefix + ".*")) System.IO.File.Delete(path);
    }

    private bool TryUserId(out Guid userId) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    private static string ContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", ".gif" => "image/gif",
        ".mp4" => "video/mp4", ".webm" => "video/webm", ".mov" => "video/quicktime", ".m4v" => "video/x-m4v", _ => "application/octet-stream"
    };
}
