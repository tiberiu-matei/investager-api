﻿using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services
{
    public class AlpacaService : IDataProviderService
    {
        private const int MaxHistoryYears = 5;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly ITimeSeriesRepository _timeSeriesRepository;
        private readonly ITimeHelper _timeHelper;
        private readonly ICache _cache;
        private readonly ITimeSeriesService _timeSeriesService;

        public AlpacaService(
            IHttpClientFactory httpClientFactory,
            ICoreUnitOfWork coreUnitOfWork,
            ITimeSeriesRepository timeSeriesPointRepository,
            ITimeHelper timeHelper,
            ICache cache,
            ITimeSeriesService timeSeriesService)
        {
            _httpClientFactory = httpClientFactory;
            _coreUnitOfWork = coreUnitOfWork;
            _timeSeriesRepository = timeSeriesPointRepository;
            _timeHelper = timeHelper;
            _cache = cache;
            _timeSeriesService = timeSeriesService;
        }

        public async Task<IEnumerable<Asset>> ScanAssets()
        {
            var client = _httpClientFactory.CreateClient(HttpClients.AlpacaPaper);
            var alpacaAssets = await client.GetFromJsonAsync<IEnumerable<AlpacaAsset>>("v2/assets") ?? new List<AlpacaAsset>();
            alpacaAssets = alpacaAssets.Where(e => e.Status == "active").ToList();

            var currentAssets = await _coreUnitOfWork.Assets.GetAll();

            var addedAssets = new List<Asset>();

            foreach (var alpacaAsset in alpacaAssets)
            {
                var exists = currentAssets.FirstOrDefault(e => e.Symbol == alpacaAsset.Symbol && e.Exchange == alpacaAsset.Exchange) != null;
                if (!exists)
                {
                    var asset = new Asset
                    {
                        Symbol = alpacaAsset.Symbol,
                        Exchange = alpacaAsset.Exchange,
                        Name = alpacaAsset.Name,
                        Provider = DataProviders.Alpaca,
                        Currency = "USD",
                    };

                    _coreUnitOfWork.Assets.Insert(asset);
                    addedAssets.Add(asset);
                }
            }

            if (addedAssets.Any())
            {
                await _coreUnitOfWork.SaveChanges();
                await _cache.Clear(CacheKeys.AssetDtos);
            }

            return addedAssets;
        }

        public async Task UpdateTimeSeriesData(string exchange, string symbol)
        {
            var utcNow = _timeHelper.GetUtcNow();

            var earliestAllowedTime = new DateTime(utcNow.Ticks - new DateTime(MaxHistoryYears + 1, 1, 1).Ticks, DateTimeKind.Utc).AddMinutes(1);

            var key = $"{exchange}:{symbol}";
            var timeSeries = await _timeSeriesService.Get(key);

            var from = timeSeries.Points.FirstOrDefault()?.Time ?? DateTime.MinValue;
            if (from < earliestAllowedTime)
            {
                from = earliestAllowedTime;
            }

            var to = utcNow - TimeSpan.FromHours(1);

            if (to < from || to - from < TimeSpan.FromDays(1))
            {
                return;
            }

            var client = _httpClientFactory.CreateClient(HttpClients.AlpacaData);
            var queryString = $"start={from:O}&end={to:O}&timeframe=1Day&limit=10000";
            var barsResponse = await client.GetFromJsonAsync<AlpacaBarsResponse>($"v2/stocks/{symbol}/bars?{queryString}") ?? new AlpacaBarsResponse();

            if (barsResponse.Bars.Any())
            {
                var assetPrices = barsResponse.Bars.Select(e => new TimeSeriesPoint { Time = e.Time, Key = key, Value = e.Close }).ToList();
                await _timeSeriesRepository.InsertRange(assetPrices);

                await _cache.Clear(key);
            }
        }
    }
}
