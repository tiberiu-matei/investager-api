using Investager.Infrastructure.Logging;
using Investager.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Investager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;

    public LogController(ILogger<LogController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Log([FromBody] LogRequest request)
    {
        switch (request.Level)
        {
            case UILogLevel.Information:
                _logger.LogInformation(request.Message);
                break;

            case UILogLevel.Debug:
                _logger.LogDebug(request.Message);
                break;

            case UILogLevel.Warning:
                _logger.LogWarning(request.Message);
                break;

            case UILogLevel.Error:
                _logger.LogError(request.Message);
                break;

            default:
                _logger.LogInformation(request.Message);
                break;
        }

        return NoContent();
    }
}
