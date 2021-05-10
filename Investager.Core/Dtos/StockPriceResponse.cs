using System;

namespace Investager.Core.Dtos
{
    public class StockPriceResponse
    {
        public DateTime Time { get; set; }

        public float Price { get; set; }
    }
}
