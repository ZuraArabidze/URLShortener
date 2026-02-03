using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URLShortener.Core.Interfaces;

namespace URLShortener.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(IUrlService service, ILogger<RedirectController> logger)
        {
            _urlService = service;
            _logger = logger;
        }

        [HttpGet("/{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            _logger.LogInformation("Redirect request received for short code {ShortCode}", shortCode);

            var url = await _urlService.GetShortenedUrlDetailsAsync(shortCode);

            if (url == null)
            {
                _logger.LogWarning("ShortCode {ShortCode} not found for redirection", shortCode);
                return NotFound();
            }

            _logger.LogInformation("Redirecting to original URL for short code {ShortCode}", shortCode);

            return Redirect(url.OriginalUrl);
        }
    }
}
