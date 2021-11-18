using Investager.Core.Constants;
using Investager.Core.Extensions;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Investager.Api.HostedServices;

public class CurrencyPairScanService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DataScanSettings _dataScanSettings;
    private readonly ICache _cache;

    public CurrencyPairScanService(
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
        StartCurrencyPairScanning(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void StartCurrencyPairScanning(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var currencyPairDataServices = scope.ServiceProvider.GetServices<ICurrencyPairDataService>();
            var tasks = currencyPairDataServices.Select(service =>
            {
                return Task.Run(async () => await ScanCurrencyPairs(service, cancellationToken));
            });

            await Task.WhenAll(tasks);
        });
    }

    private async Task ScanCurrencyPairs(ICurrencyPairDataService dataService, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
                var currentPairs = await currencyService.GetPairs();
                var currencies = await currencyService.GetAll();

                var pairs = await dataService.GetPairs();

                var pairsToAdd = pairs
                    .Where(e => !currentPairs.Any(x => x.GetKey() == e.GetKey()))
                    .ToList();

                var coreUnitOfWork = scope.ServiceProvider.GetRequiredService<ICoreUnitOfWork>();
                pairsToAdd.ForEach(async pair =>
                {
                    var firstCurrency = currencies.SingleOrDefault(e => e.Code == pair.FirstCurrency.Code);
                    if (firstCurrency == null)
                    {
                        await AddNewCurrency(coreUnitOfWork, pair.FirstCurrency);

                        var updatedCurrencies = await currencyService.GetAll();
                        pair.FirstCurrencyId = updatedCurrencies.Single(e => e.Code == pair.FirstCurrency.Code).Id;
                    }
                    else
                    {
                        pair.FirstCurrencyId = firstCurrency.Id;
                    }

                    pair.FirstCurrency = null;

                    var secondCurrency = currencies.SingleOrDefault(e => e.Code == pair.SecondCurrency.Code);
                    if (secondCurrency == null)
                    {
                        await AddNewCurrency(coreUnitOfWork, pair.SecondCurrency);

                        var updatedCurrencies = await currencyService.GetAll();
                        pair.SecondCurrencyId = updatedCurrencies.Single(e => e.Code == pair.SecondCurrency.Code).Id;
                    }
                    else
                    {
                        pair.SecondCurrencyId = secondCurrency.Id;
                    }

                    pair.SecondCurrency = null;

                    var reversePair = GetReversePair(pair);

                    coreUnitOfWork.CurrencyPairs.Add(pair);
                    coreUnitOfWork.CurrencyPairs.Add(reversePair);
                });

                var pairsToRemove = currentPairs
                    .Where(e => e.Provider == dataService.Provider
                        && e.HasTimeData
                        && !pairs.Any(x => x.GetKey() == e.GetKey()))
                    .ToList();

                pairsToRemove.ForEach(async e =>
                {
                    var reversePair = currentPairs
                        .Single(x => x.Provider == dataService.Provider
                            && x.GetKey() == GetReversePair(e).GetKey());

                    coreUnitOfWork.CurrencyPairs.Delete(e);
                    coreUnitOfWork.CurrencyPairs.Delete(reversePair);
                    var key = e.GetKey();

                    var timeSeriesRepository = scope.ServiceProvider.GetRequiredService<ITimeSeriesRepository>();
                    await timeSeriesRepository.DeleteSeries(key);
                    await _cache.Clear(key);
                });

                if (pairsToAdd.Any() || pairsToRemove.Any())
                {
                    await coreUnitOfWork.SaveChanges();
                    await ClearCurrencyCaches();
                }
            }

            await Task.Delay(_dataScanSettings.ScanInterval);
        }
    }

    private CurrencyPair GetReversePair(CurrencyPair currencyPair)
    {
        return new CurrencyPair
        {
            FirstCurrency = currencyPair.SecondCurrency,
            FirstCurrencyId = currencyPair.SecondCurrencyId,
            SecondCurrency = currencyPair.FirstCurrency,
            SecondCurrencyId = currencyPair.FirstCurrencyId,
            Provider = currencyPair.Provider,
            HasTimeData = !currencyPair.HasTimeData,
        };
    }

    private async Task AddNewCurrency(ICoreUnitOfWork coreUnitOfWork, Currency currency)
    {
        coreUnitOfWork.Currencies.Add(currency);
        await coreUnitOfWork.SaveChanges();
        await ClearCurrencyCaches();
    }

    private async Task ClearCurrencyCaches()
    {
        await _cache.Clear(CacheKeys.CurrencyPairs);
        await _cache.Clear(CacheKeys.Currencies);
    }
}
