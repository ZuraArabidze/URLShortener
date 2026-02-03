using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URLShortener.API.DTOs;
using URLShortener.Core.Interfaces;

namespace URLShortener.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly ILogger<UrlsController> _logger;

        public UrlsController(IUrlService service, ILogger<UrlsController> logger)
        {
            _urlService = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShortUrlResponse>>> GetAllUrls([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Fetching all shortened URLs - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var urls = await _urlService.GetAllShortenedUrlsAsync(pageNumber, pageSize);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = urls.Select(url => new ShortUrlResponse
            {
                Id = url.Id,
                ShortCode = url.ShortCode,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = $"{baseUrl}/{url.ShortCode}",
                CreatedAt = url.CreatedAt,
                ExpiresAt = url.ExpiresAt,
                ClickCount = url.ClickCount
            });

            return Ok(response);
        }

        [HttpGet("{shortCode}")]
        public async Task<ActionResult<ShortUrlResponse>> GetUrlDetails(string shortCode)
        {
            _logger.LogInformation("Fetching details for short code {ShortCode}", shortCode);

            var url = await _urlService.GetShortenedUrlDetailsAsync(shortCode);

            if (url == null)
            {
                _logger.LogWarning("ShortCode {ShortCode} not found", shortCode);
                return NotFound();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = new ShortUrlResponse
            {
                Id = url.Id,
                ShortCode = url.ShortCode,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = $"{baseUrl}/{url.ShortCode}",
                CreatedAt = url.CreatedAt,
                ExpiresAt = url.ExpiresAt,
                ClickCount = url.ClickCount
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl([FromBody] CreateShortUrlRequest request)
        {
            _logger.LogInformation("Creating short URL for {OriginalUrl}", request.OriginalUrl);

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var shortenedUrl = await _urlService.CreateShortUrlAsync(
                request.OriginalUrl,
                request.CustomShortCode,
                request.ExpiresAt,
                clientIp);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = new ShortUrlResponse
            {
                Id = shortenedUrl.Id,
                ShortCode = shortenedUrl.ShortCode,
                OriginalUrl = shortenedUrl.OriginalUrl,
                ShortUrl = $"{baseUrl}/{shortenedUrl.ShortCode}",
                CreatedAt = shortenedUrl.CreatedAt,
                ExpiresAt = shortenedUrl.ExpiresAt,
                ClickCount = shortenedUrl.ClickCount
            };

            _logger.LogInformation("Short URL created with ShortCode {ShortCode}", response.ShortCode);

            return CreatedAtAction(nameof(GetUrlDetails), new { shortCode = response.ShortCode }, response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteShortUrl(Guid id)
        {
            _logger.LogInformation("Deleting short URL with ID {Id}", id);

            var deleted = await _urlService.DeleteShortUrlAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Short URL with ID {Id} not found for deletion", id);
                return NotFound();
            }

            _logger.LogInformation("Short URL with ID {Id} deleted successfully", id);

            return NoContent();
        }
    }
}
