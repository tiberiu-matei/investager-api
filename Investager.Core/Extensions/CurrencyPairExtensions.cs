using Investager.Core.Models;

namespace Investager.Core.Extensions;

public static class CurrencyPairExtensions
{
    public static string GetKey(this CurrencyPair currencyPair)
    {
        return $"{currencyPair.FirstCurrency.Code}:{currencyPair.SecondCurrency.Code}";
    }
}
