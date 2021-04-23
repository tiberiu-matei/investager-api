using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface ITimeSeriesPointRepository
    {
        Task<IEnumerable<TimeSeriesPoint>> GetAsync(string key, DateTime from);

        Task<IEnumerable<TimeSeriesPoint>> GetAsync(string key, DateTime from, DateTime to);

        Task InsertRangeAsync(IEnumerable<TimeSeriesPoint> timeSeriesPoints);
    }
}
