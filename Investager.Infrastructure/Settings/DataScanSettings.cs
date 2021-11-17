using System;

namespace Investager.Infrastructure.Settings
{
    public class DataScanSettings
    {
        public TimeSpan ScanInterval { get; set; } = TimeSpan.FromHours(4);
    }
}
