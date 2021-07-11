using Serilog.Events;

namespace Investager.Infrastructure.Models
{
    public class LogRequest
    {
        public LogEventLevel Level { get; set; }

        public string Message { get; set; }
    }
}
