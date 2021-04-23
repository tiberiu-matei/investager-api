using System;

namespace Investager.Core.Models
{
    public class TimeSeriesPoint
    {
        public DateTime Time { get; set; }

        public string Key { get; set; } = default!;

        public float Value { get; set; }
    }
}
