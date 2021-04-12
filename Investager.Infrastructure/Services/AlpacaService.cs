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
        private readonly ITimeSeriesUnitOfWork _timeSeriesUnitOfWork;
        private readonly ITimeHelper _timeHelper;

        public AlpacaService(
            IHttpClientFactory httpClientFactory,
            ICoreUnitOfWork coreUnitOfWork,
            ITimeSeriesUnitOfWork timeSeriesUnitOfWork,
            ITimeHelper timeHelper)
        {
            _httpClientFactory = httpClientFactory;
            _coreUnitOfWork = coreUnitOfWork;
            _timeSeriesUnitOfWork = timeSeriesUnitOfWork;
            _timeHelper = timeHelper;
        }

        public async Task<IEnumerable<Asset>> ScanAssetsAsync()
        {
            var client = _httpClientFactory.CreateClient(HttpClients.Alpaca);
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
                await _coreUnitOfWork.SaveChangesAsync();
            }

            return addedAssets;
        }

        public async Task UpdateTimeSeriesDataAsync()
        {
            Func<IQueryable<Asset>, IOrderedQueryable<Asset>> orderBy = query => query.OrderBy(e => e.LastPriceUpdate);
            var assetsToUpdate = await _coreUnitOfWork.Assets.Find(e => e.Provider == DataProviders.Alpaca, orderBy, 1);
            var assetToUpdate = assetsToUpdate.FirstOrDefault();
            if (assetToUpdate != null)
            {
                var utcNow = _timeHelper.GetUtcNow();

                var earliestAllowedTime = new DateTime(utcNow.Ticks - new DateTime(MaxHistoryYears + 1, 1, 1).Ticks, DateTimeKind.Utc).AddMinutes(1);

                var from = assetToUpdate.LastPriceUpdate;
                if (from == null || from < earliestAllowedTime)
                {
                    from = earliestAllowedTime;
                }

                var client = _httpClientFactory.CreateClient(HttpClients.Alpaca);
                var queryString = $"start={from.Value:O}&end={utcNow:O}&timeframe=1Day&limit=10000";
                var barsResponse = await client.GetFromJsonAsync<AlpacaBarsResponse>($"v2/stocks/{assetToUpdate.Symbol}/bars?{queryString}") ?? new AlpacaBarsResponse();

                var assetPrices = barsResponse.Bars.Select(e => new AssetPrice { Time = e.Time, Key = $"{assetToUpdate.Exchange}:{assetToUpdate.Symbol}", Price = e.Close }).ToList();
                assetPrices.ForEach(e => _timeSeriesUnitOfWork.AssetPrices.Insert(e));

                await _timeSeriesUnitOfWork.SaveChangesAsync();

                assetToUpdate.LastPriceUpdate = utcNow;
                _coreUnitOfWork.Assets.Update(assetToUpdate);
                await _coreUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
