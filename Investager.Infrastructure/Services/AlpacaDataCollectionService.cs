using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
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
                        using var scope = _serviceScopeFactory.CreateScope();
                        var dataProviderServiceFactory = scope.ServiceProvider.GetRequiredService<IDataProviderServiceFactory>();
                        var alpacaService = dataProviderServiceFactory.CreateService(DataProviders.Alpaca);
                        await alpacaService.UpdateTimeSeriesDataAsync();

                        await Task.Delay(_alpacaSettings.PeriodBetweenDataRequests);
                    }
                });
            }
        }

        public void Stop()
        {
            _active = false;
        }
    }
}
