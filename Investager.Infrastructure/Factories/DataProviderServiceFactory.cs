using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Services;
using System.Net.Http;

namespace Investager.Infrastructure.Factories
{
    public class DataProviderServiceFactory : IDataProviderServiceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly ITimeSeriesUnitOfWork _timeSeriesUnitOfWork;
        private readonly ITimeHelper _timeHelper;

        public DataProviderServiceFactory(
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

        public IDataProviderService CreateService(string provider)
        {
            return new AlpacaService(_httpClientFactory, _coreUnitOfWork, _timeSeriesUnitOfWork, _timeHelper);
        }
    }
}
