namespace Investager.Core.Dtos;

public class WatchedCurrencyPairResponse
{
    public string FirstCurrencyName { get; set; }

    public string SecondCurrencyName { get; set; }

    public string Key { get; set; }

    public int DisplayOrder { get; set; }

    public GainLossResponse GainLoss { get; set; }
}
