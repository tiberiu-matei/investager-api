using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class TimeSeriesService : ITimeSeriesService
    {
        private readonly ITimeSeriesRepository _timeSeriesRepository;
        private readonly ICache _cache;

        public TimeSeriesService(ITimeSeriesRepository timeSeriesRepository, ICache cache)
        {
            _timeSeriesRepository = timeSeriesRepository;
            _cache = cache;
        }

        public Task<TimeSeriesResponse> Get(string key)
        {
            return _cache.Get(key, async () => await _timeSeriesRepository.Get(key));
        }
    }
}
