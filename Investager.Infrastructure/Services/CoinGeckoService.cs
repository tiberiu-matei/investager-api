using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Infrastructure.Models.CoinGecko;
using Investager.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services;

public class CoinGeckoService : ICurrencyPairDataService
{
    private const double MinimumUsdMarketCap = 1000000f;
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITimeHelper _timeHelper;
    private readonly CoinGeckoSettings _coinGeckoSettings;

    public CoinGeckoService(
        IHttpClientFactory httpClientFactory,
        ITimeHelper timeHelper,
        CoinGeckoSettings coinGeckoSettings)
    {
        _httpClientFactory = httpClientFactory;
        _timeHelper = timeHelper;
        _coinGeckoSettings = coinGeckoSettings;
    }

    public string Provider => DataProviders.CoinGecko;

    public TimeSpan DataQueryInterval => _coinGeckoSettings.DataQueryInterval;

    public async Task<IEnumerable<CurrencyPair>> GetPairs()
    {
        var client = _httpClientFactory.CreateClient(HttpClients.CoinGecko);
        var coins = await client.GetFromJsonAsync<IEnumerable<CoinGeckoCoin>>("v3/coins/list");

        var usdCurrency = new Currency
        {
            Code = "usd",
            Name = "American Dollar",
            ProviderId = "usd",
            Type = CurrencyType.Fiat,
        };

        var pairs = new List<CurrencyPair>();
        foreach (var coin in coins)
        {
            var marketChart = await client
                .GetFromJsonAsync<CoinGeckoMarketChart>($"v3/coins/{coin.Id}/market_chart?vs_currency=usd&days=2");

            var latestMarketCap = Convert.ToDouble(marketChart.MarketCaps.Last().Last().ToString());

            if (latestMarketCap > MinimumUsdMarketCap)
            {
                pairs.Add(new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = coin.Symbol,
                        Name = coin.Name,
                        ProviderId = coin.Id,
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = usdCurrency,
                    Provider = DataProviders.CoinGecko,
                    HasTimeData = true,
                });
            }

            await Task.Delay(DataQueryInterval);
        }

        return pairs;
    }

    public async Task<IEnumerable<TimeSeriesPoint>> GetRecentPoints(UpdateCurrencyPairDataRequest request)
    {
        var client = _httpClientFactory.CreateClient(HttpClients.CoinGecko);
        string days;
        if (request.LatestPointTime != null)
        {
            var utcNow = _timeHelper.GetUtcNow();
            var totalDays = (utcNow - request.LatestPointTime.Value).TotalDays;

            if (totalDays == 0)
            {
                return new List<TimeSeriesPoint>();
            }

            if (totalDays < 91)
            {
                totalDays = 91;
            }

            days = totalDays.ToString();
        }
        else
        {
            days = "max";
        }

        var urlPath = $"v3/coins/{request.FirstCurrencyProviderId}/market_chart?vs_currency={request.SecondCurrencyProviderId}&days={days}";

        var marketChart = await client.GetFromJsonAsync<CoinGeckoMarketChart>(urlPath);

        var points = marketChart.Prices
            .Select(e => new TimeSeriesPoint
            {
                Time = UnixEpoch.AddMilliseconds(Convert.ToInt64(e.First().ToString())),
                Key = request.Key,
                Value = Convert.ToSingle(e.Last().ToString()),
            })
            .Where(e => e.Time > (request.LatestPointTime ?? DateTime.MinValue))
            .ToList();

        return points;
    }
}
