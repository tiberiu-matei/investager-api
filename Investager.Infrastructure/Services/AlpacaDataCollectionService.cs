using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Services
{
    public class AlpacaDataCollectionService : IDataCollectionService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AlpacaSettings _alpacaSettings;

        private bool _active;

        public AlpacaDataCollectionService(
            IServiceScopeFactory serviceScopeFactory,
            AlpacaSettings alpacaSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _alpacaSettings = alpacaSettings;
        }

        public void Start()
        {
            if (!_active)
            {
                _active = true;

                Task.Run(async () =>
                {
                    while (_active)
                    {
                        await UpdateAssetsData();

                        await Task.Delay(_alpacaSettings.PeriodBetweenDataRequestBathes);
                    }
                });
            }
        }

        private async Task UpdateAssetsData()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var coreUnitOfWork = scope.ServiceProvider.GetRequiredService<ICoreUnitOfWork>();
            var assets = await coreUnitOfWork.Assets.GetAllTracked();
            var orderedAssets = assets.OrderBy(e => e.LastPriceUpdate).ToList();

            foreach (var asset in orderedAssets)
            {
                if (!_active)
                {
                    return;
                }

                var dataProviderServiceFactory = scope.ServiceProvider.GetRequiredService<IDataProviderServiceFactory>();
                var alpacaService = dataProviderServiceFactory.CreateService(DataProviders.Alpaca);

                try
                {
                    await alpacaService.UpdateTimeSeriesData(asset);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating asset data. {ex.Message}");
                }

                await Task.Delay(_alpacaSettings.PeriodBetweenDataRequests);
            }
        }

        public void Stop()
        {
            _active = false;
        }
    }
}
