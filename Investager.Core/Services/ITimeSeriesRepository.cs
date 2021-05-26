using Investager.Core.Dtos;
using Investager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface ITimeSeriesRepository
    {
        Task<TimeSeriesResponse> Get(string key);

        Task InsertRange(IEnumerable<TimeSeriesPoint> timeSeriesPoints);
    }
}
