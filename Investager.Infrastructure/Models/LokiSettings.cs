using System;

namespace Investager.Infrastructure.Models
{
    public class LokiSettings
    {
        public TimeSpan BatchInterval { get; set; } = TimeSpan.FromMinutes(1);

        public int MaxBatchSize { get; set; } = 100;
    }
}
