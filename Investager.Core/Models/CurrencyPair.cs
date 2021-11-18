namespace Investager.Core.Models;

public class CurrencyPair
{
    public int FirstCurrencyId { get; set; }

    public Currency FirstCurrency { get; set; }

    public int SecondCurrencyId { get; set; }

    public Currency SecondCurrency { get; set; }

    public string Provider { get; set; }

    public bool HasTimeData { get; set; }
}
