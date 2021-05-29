using System;

namespace Investager.Infrastructure.Models
{
    public class AlpacaSettings
    {
        public TimeSpan PeriodBetweenDataRequests { get; set; } = TimeSpan.FromSeconds(2);

        public TimeSpan PeriodBetweenDataRequestBathes { get; set; } = TimeSpan.FromMinutes(30);
    }
}
