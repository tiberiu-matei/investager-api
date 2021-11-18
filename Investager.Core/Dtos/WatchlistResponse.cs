using System.Collections.Generic;

namespace Investager.Core.Dtos;

public class WatchlistResponse
{
    public int Id { get; set; }

    public IEnumerable<WatchedAssetResponse> Assets { get; set; }

    public IEnumerable<WatchedCurrencyPairResponse> CurrencyPairs { get; set; }
}
