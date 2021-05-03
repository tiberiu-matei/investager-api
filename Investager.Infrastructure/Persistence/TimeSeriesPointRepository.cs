using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence
{
    public class TimeSeriesPointRepository : ITimeSeriesPointRepository
    {
        private readonly InvestagerTimeSeriesContext _context;

        public TimeSeriesPointRepository(InvestagerTimeSeriesContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<TimeSeriesPoint>> GetAsync(string key, DateTime from)
        {
            return GetAsync(key, from, DateTime.UtcNow);
        }

        public Task<IEnumerable<TimeSeriesPoint>> GetAsync(string key, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public async Task InsertRangeAsync(IEnumerable<TimeSeriesPoint> timeSeriesPoints)
        {
            if (!timeSeriesPoints.Any())
            {
                return;
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("INSERT INTO \"TimeSeriesPoint\"(\"Time\", \"Key\", \"Value\")");
            sqlBuilder.AppendLine("VALUES");
            foreach (var timeSeriesPoint in timeSeriesPoints)
            {
                sqlBuilder.AppendLine($"('{timeSeriesPoint.Time:yyyy-MM-dd HH:mm:ss.ffffff}', '{timeSeriesPoint.Key}', {timeSeriesPoint.Value}),");
            }

            sqlBuilder.Remove(sqlBuilder.Length - 3, 3);
            sqlBuilder.Append(';');

            Console.WriteLine(sqlBuilder.ToString());

            await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString());
        }
    }
}
