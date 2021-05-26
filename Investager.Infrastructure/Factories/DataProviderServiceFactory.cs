﻿using Investager.Core.Interfaces;
using Investager.Core.Services;
using Investager.Infrastructure.Services;
using System.Net.Http;

namespace Investager.Infrastructure.Factories
{
    public class DataProviderServiceFactory : IDataProviderServiceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly ITimeSeriesRepository _timeSeriesPointRepository;
        private readonly ITimeHelper _timeHelper;
        private readonly ICache _cache;

        public DataProviderServiceFactory(
            IHttpClientFactory httpClientFactory,
            ICoreUnitOfWork coreUnitOfWork,
            ITimeSeriesRepository timeSeriesPointRepository,
            ITimeHelper timeHelper,
            ICache cache)
        {
            _httpClientFactory = httpClientFactory;
            _coreUnitOfWork = coreUnitOfWork;
            _timeSeriesPointRepository = timeSeriesPointRepository;
            _timeHelper = timeHelper;
            _cache = cache;
        }

        public IDataProviderService CreateService(string provider)
        {
            return new AlpacaService(_httpClientFactory, _coreUnitOfWork, _timeSeriesPointRepository, _timeHelper, _cache);
        }
    }
}
