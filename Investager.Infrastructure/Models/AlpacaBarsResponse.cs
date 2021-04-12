using System.Collections.Generic;

namespace Investager.Infrastructure.Models
{
    public class AlpacaBarsResponse
    {
        public IEnumerable<AlpacaAssetPrice> Bars { get; set; } = new List<AlpacaAssetPrice>();
    }
}
