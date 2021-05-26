﻿using System;
using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Provider { get; set; }

        public string Symbol { get; set; }

        public string Exchange { get; set; }

        public string Name { get; set; }

        public string Industry { get; set; }

        public string Currency { get; set; }

        public DateTime? LastPriceUpdate { get; set; }

        public float? LastPrice { get; set; }

        public ICollection<PortfolioAsset> PortfolioAssets = new List<PortfolioAsset>();
    }
}
