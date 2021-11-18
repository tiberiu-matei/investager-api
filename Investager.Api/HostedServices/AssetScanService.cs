using Investager.Core.Constants;
using Investager.Core.Extensions;
using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Investager.Api.HostedServices;

public class AssetScanService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DataScanSettings _dataScanSettings;
    private readonly ICache _cache;

    public AssetScanService(
        IServiceScopeFactory serviceScopeFactory,
        DataScanSettings dataScanSettings,
        ICache cache)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _dataScanSettings = dataScanSettings;
        _cache = cache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartAssetScanning(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void StartAssetScanning(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var assetDataServices = scope.ServiceProvider.GetServices<IAssetDataService>();
            var tasks = assetDataServices.Select(service =>
            {
                return Task.Run(async () => await ScanAssets(service, cancellationToken));
            });

            await Task.WhenAll(tasks);
        });
    }

    private async Task ScanAssets(IAssetDataService dataService, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var assets = await dataService.GetAssets();
                var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();
                var currentAssets = await assetService.GetAssets();

                var assetsToAdd = assets
                    .Where(e => !currentAssets.Any(x => x.GetKey() == e.GetKey()))
                    .ToList();

                var coreUnitOfWork = scope.ServiceProvider.GetRequiredService<ICoreUnitOfWork>();
                assetsToAdd.ForEach(e => coreUnitOfWork.Assets.Add(e));

                var assetsToRemove = currentAssets
                    .Where(e => e.Provider == dataService.Provider
                        && !assets.Any(x => x.GetKey() == e.GetKey()))
                    .ToList();

                assetsToRemove.ForEach(async e =>
                {
                    coreUnitOfWork.Assets.Delete(e);
                    var key = e.GetKey();

                    var timeSeriesRepository = scope.ServiceProvider.GetRequiredService<ITimeSeriesRepository>();
                    await timeSeriesRepository.DeleteSeries(key);
                    await _cache.Clear(key);
                });

                if (assetsToAdd.Any() || assetsToRemove.Any())
                {
                    await coreUnitOfWork.SaveChanges();
                    await _cache.Clear(CacheKeys.AssetDtos);
                    await _cache.Clear(CacheKeys.Assets);
                }
            }

            await Task.Delay(_dataScanSettings.ScanInterval);
        }
    }
}
