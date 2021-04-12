using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Investager.Infrastructure.Factories
{
    public class DataCollectionServiceFactory : IDataCollectionServiceFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AlpacaSettings _alpacaSettings;

        private AlpacaDataCollectionService? _alpacaDataCollectionService;

        public DataCollectionServiceFactory(
            IServiceScopeFactory serviceScopeFactory,
            AlpacaSettings alpacaSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _alpacaSettings = alpacaSettings;
        }

        public IDataCollectionService GetService(string provider)
        {
            if (_alpacaDataCollectionService == null)
            {
                _alpacaDataCollectionService = new AlpacaDataCollectionService(
                    _serviceScopeFactory,
                    _alpacaSettings);
            }

            return _alpacaDataCollectionService;
        }
    }
}
