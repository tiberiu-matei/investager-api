using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Investager.Infrastructure.Models.CoinGecko
{
    public class CoinGeckoMarketChart
    {
        [JsonPropertyName("prices")]
        public IEnumerable<IEnumerable<object>> Prices { get; set; }

        [JsonPropertyName("market_caps")]
        public IEnumerable<IEnumerable<object>> MarketCaps { get; set; }

        [JsonPropertyName("total_volumes")]
        public IEnumerable<IEnumerable<object>> TotalVolumes { get; set; }
    }
}
