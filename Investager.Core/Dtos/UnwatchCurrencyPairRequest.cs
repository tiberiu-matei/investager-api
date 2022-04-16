namespace Investager.Core.Dtos;

public class UnwatchCurrencyPairRequest
{
    public int UserId { get; set; }

    public int WatchlistId { get; set; }

    public int FirstCurrencyId { get; set; }

    public int SecondCurrencyId { get; set; }
}
