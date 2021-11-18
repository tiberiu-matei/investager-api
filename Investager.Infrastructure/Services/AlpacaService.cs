using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services;

public class AlpacaService : IAssetDataService
{
    private const int MaxHistoryYears = 5;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITimeHelper _timeHelper;

    public AlpacaService(
        IHttpClientFactory httpClientFactory,
        ITimeHelper timeHelper)
    {
        _httpClientFactory = httpClientFactory;
        _timeHelper = timeHelper;
    }

    public string Provider { get; } = DataProviders.Alpaca;

    public TimeSpan DataQueryInterval { get; } = TimeSpan.FromSeconds(10);

    public async Task<IEnumerable<Asset>> GetAssets()
    {
        var client = _httpClientFactory.CreateClient(HttpClients.AlpacaPaper);
        var alpacaAssets = await client.GetFromJsonAsync<IEnumerable<AlpacaAsset>>("v2/assets") ?? new List<AlpacaAsset>();
        alpacaAssets = alpacaAssets.Where(e => e.Status == "active").ToList();

        var assets = alpacaAssets.Select(e =>
        {
            return new Asset
            {
                Symbol = e.Symbol,
                Exchange = e.Exchange,
                Name = e.Name,
                Provider = DataProviders.Alpaca,
                Currency = "USD",
            };
        }).ToList();

        return assets;
    }

    public async Task<IEnumerable<TimeSeriesPoint>> GetRecentPoints(UpdateAssetDataRequest request)
    {
        var utcNow = _timeHelper.GetUtcNow();

        var earliestAllowedTime = new DateTime(utcNow.Ticks - new DateTime(MaxHistoryYears + 1, 1, 1).Ticks, DateTimeKind.Utc)
            .AddMinutes(1);

        var from = request.LatestPointTime ?? DateTime.MinValue;
        if (from < earliestAllowedTime)
        {
            from = earliestAllowedTime;
        }

        var to = utcNow - TimeSpan.FromHours(1);

        if (to < from || to - from < TimeSpan.FromDays(1))
        {
            return new List<TimeSeriesPoint>();
        }

        var client = _httpClientFactory.CreateClient(HttpClients.AlpacaData);
        var queryString = $"start={from:O}&end={to:O}&timeframe=1Day&limit=10000";
        var barsResponse = await client.GetFromJsonAsync<AlpacaBarsResponse>($"v2/stocks/{request.Symbol}/bars?{queryString}")
            ?? new AlpacaBarsResponse();

        return barsResponse.Bars.Any()
            ? barsResponse.Bars
                .Select(e => new TimeSeriesPoint { Time = e.Time, Key = request.Key, Value = e.Close })
                .Where(e => e.Time > (request.LatestPointTime ?? DateTime.MinValue))
                .ToList()
            : new List<TimeSeriesPoint>();
    }
}
