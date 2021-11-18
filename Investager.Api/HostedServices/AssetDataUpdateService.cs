using Investager.Core.Dtos;
using Investager.Core.Extensions;
using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Investager.Api.HostedServices;

public class AssetDataUpdateService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICache _cache;
    private readonly DataUpdateSettings _dataUpdateSettings;
    private readonly ILogger<AssetDataUpdateService> _logger;

    public AssetDataUpdateService(
        IServiceScopeFactory serviceScopeFactory,
        ICache cache,
        DataUpdateSettings dataUpdateSettings,
        ILogger<AssetDataUpdateService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _cache = cache;
        _dataUpdateSettings = dataUpdateSettings;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartDataAcquisitionTasks(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void StartDataAcquisitionTasks(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var assetDataServices = scope.ServiceProvider.GetServices<IAssetDataService>();
            var tasks = assetDataServices.Select(service =>
            {
                return Task.Run(async () => await UpdateTimeSeries(service, cancellationToken));
            });

            await Task.WhenAll(tasks);
        });
    }

    private async Task UpdateTimeSeries(IAssetDataService dataService, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();
                var assets = await assetService.GetAssets();

                var assetsToUpdate = assets
                    .Where(e => e.Provider == dataService.Provider)
                    .ToList();

                foreach (var assetToUpdate in assetsToUpdate)
                {
                    try
                    {
                        var currentAssets = await assetService.GetAssets();
                        var asset = currentAssets.Single(e => e.Id == assetToUpdate.Id);
                        var key = asset.GetKey();

                        var timeSeriesService = scope.ServiceProvider.GetRequiredService<ITimeSeriesService>();
                        var timeSeries = await timeSeriesService.Get(key);
                        var latestPointTime = timeSeries.Points.FirstOrDefault()?.Time;

                        var request = new UpdateAssetDataRequest
                        {
                            Exchange = asset.Exchange,
                            Symbol = asset.Symbol,
                            Key = key,
                            LatestPointTime = latestPointTime,
                        };

                        var recentPoints = await dataService.GetRecentPoints(request);

                        if (recentPoints.Any())
                        {
                            var timeSeriesRepository = scope.ServiceProvider.GetRequiredService<ITimeSeriesRepository>();
                            await timeSeriesRepository.InsertRange(recentPoints);
                            await _cache.Clear(key);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating asset data");
                    }
                    finally
                    {
                        await Task.Delay(dataService.DataQueryInterval);
                    }
                }
            }

            await Task.Delay(_dataUpdateSettings.UpdateInterval);
        }
    }
}
