using System;
using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Provider { get; set; } = default!;

        public string Symbol { get; set; } = default!;

        public string Exchange { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string? Industry { get; set; }

        public string Currency { get; set; } = default!;

        public DateTime? LastPriceUpdate { get; set; }

        public ICollection<Portfolio> Portfolios = new List<Portfolio>();
    }
}
