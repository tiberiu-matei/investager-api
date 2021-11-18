using Investager.Infrastructure.Logging;

namespace Investager.Infrastructure.Models;

public class LogRequest
{
    public UILogLevel Level { get; set; }

    public string Message { get; set; }
}
