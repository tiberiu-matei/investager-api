using Investager.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Investager.Api.Controllers
{
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
                case LogEventLevel.Error:
                    _logger.LogInformation(request.Message);
                    break;
                case LogEventLevel.Warning:
                    _logger.LogWarning(request.Message);
                    break;
                case LogEventLevel.Debug:
                    _logger.LogDebug(request.Message);
                    break;
                case LogEventLevel.Information:
                case LogEventLevel.Verbose:
                case LogEventLevel.Fatal:
                default:
                    _logger.LogInformation(request.Message);
                    break;
            }

            return NoContent();
        }
    }
}
