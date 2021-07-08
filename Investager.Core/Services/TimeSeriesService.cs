using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using System.Linq;
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
            return _cache.Get(key, async () => await GetData(key));
        }

        private async Task<TimeSeriesResponse> GetData(string key)
        {
            var response = await _timeSeriesRepository.Get(key);

            response.Points = response.Points.OrderByDescending(e => e.Time);

            return response;
        }
    }
}
