using System;

namespace Investager.Core.Models
{
    public class CurrencyExchangeRatio
    {
        public DateTime Time { get; set; }

        public string Key { get; set; } = default!;

        public float Ratio { get; set; }

        public DateTime? LastRatioUpdate { get; set; }
    }
}
