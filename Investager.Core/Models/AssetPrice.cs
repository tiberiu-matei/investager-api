using System;

namespace Investager.Core.Models
{
    public class AssetPrice
    {
        public DateTime Time { get; set; }

        public string Key { get; set; } = default!;

        public float Price { get; set; }
    }
}
