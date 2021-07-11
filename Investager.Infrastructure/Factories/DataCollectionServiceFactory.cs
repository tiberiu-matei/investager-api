using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Investager.Infrastructure.Factories
{
    public class DataCollectionServiceFactory : IDataCollectionServiceFactory
    {
        private readonly ILogger<AlpacaDataCollectionService> _alpacaLogger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AlpacaSettings _alpacaSettings;

        private AlpacaDataCollectionService _alpacaDataCollectionService;

        public DataCollectionServiceFactory(
            ILogger<AlpacaDataCollectionService> alpacaLogger,
            IServiceScopeFactory serviceScopeFactory,
            AlpacaSettings alpacaSettings)
        {
            _alpacaLogger = alpacaLogger;
            _serviceScopeFactory = serviceScopeFactory;
            _alpacaSettings = alpacaSettings;
        }

        public IDataCollectionService GetService(string provider)
        {
            if (_alpacaDataCollectionService == null)
            {
                _alpacaDataCollectionService = new AlpacaDataCollectionService(
                    _alpacaLogger,
                    _serviceScopeFactory,
                    _alpacaSettings);
            }

            return _alpacaDataCollectionService;
        }
    }
}
