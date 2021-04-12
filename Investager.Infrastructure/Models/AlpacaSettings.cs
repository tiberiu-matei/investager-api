using System;

namespace Investager.Infrastructure.Models
{
    public class AlpacaSettings
    {
        public TimeSpan PeriodBetweenDataRequests { get; set; } = TimeSpan.FromSeconds(1);
    }
}
