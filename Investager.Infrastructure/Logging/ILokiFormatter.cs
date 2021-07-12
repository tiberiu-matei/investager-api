using Serilog.Events;

namespace Investager.Infrastructure.Logging
{
    public interface ILokiFormatter
    {
        string Format(LogEvent logEvent);
    }
}
