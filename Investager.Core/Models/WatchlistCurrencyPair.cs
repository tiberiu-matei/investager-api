namespace Investager.Core.Models;

public class WatchlistCurrencyPair
{
    public int WatchlistId { get; set; }

    public Watchlist Watchlist { get; set; }

    public int FirstCurrencyId { get; set; }

    public int SecondCurrencyId { get; set; }

    public CurrencyPair CurrencyPair { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsReversed { get; set; }
}
