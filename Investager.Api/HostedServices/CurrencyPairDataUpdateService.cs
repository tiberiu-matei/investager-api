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

public class CurrencyPairDataUpdateService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICache _cache;
    private readonly DataUpdateSettings _dataUpdateSettings;
    private readonly ILogger<CurrencyPairDataUpdateService> _logger;

    public CurrencyPairDataUpdateService(
        IServiceScopeFactory serviceScopeFactory,
        ICache cache,
        DataUpdateSettings dataUpdateSettings,
        ILogger<CurrencyPairDataUpdateService> logger)
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

            var currencyPairDataServices = scope.ServiceProvider.GetServices<ICurrencyPairDataService>();
            var tasks = currencyPairDataServices.Select(service =>
            {
                return Task.Run(async () => await UpdateTimeSeries(service, cancellationToken));
            });

            await Task.WhenAll(tasks);
        });
    }

    private async Task UpdateTimeSeries(ICurrencyPairDataService dataService, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
                var pairs = await currencyService.GetPairs();

                var pairsToUpdate = pairs
                    .Where(e => e.Provider == dataService.Provider && e.HasTimeData)
                    .ToList();

                foreach (var pairToUpdate in pairsToUpdate)
                {
                    try
                    {
                        var currentPairs = await currencyService.GetPairs();
                        var pair = currentPairs.Single(e => e.GetKey() == pairToUpdate.GetKey());
                        var key = pair.GetKey();

                        var timeSeriesService = scope.ServiceProvider.GetRequiredService<ITimeSeriesService>();
                        var timeSeries = await timeSeriesService.Get(key);
                        var latestPointTime = timeSeries.Points.FirstOrDefault()?.Time;

                        var request = new UpdateCurrencyPairDataRequest
                        {
                            FirstCurrencyProviderId = pair.FirstCurrency.ProviderId,
                            SecondCurrencyProviderId = pair.SecondCurrency.ProviderId,
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
                        _logger.LogError(ex, "Error updating currency pair data");
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
