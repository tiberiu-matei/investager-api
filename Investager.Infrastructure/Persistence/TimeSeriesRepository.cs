using Investager.Core.Dtos;
using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence
{
    public class TimeSeriesRepository : ITimeSeriesRepository
    {
        private readonly InvestagerTimeSeriesContext _context;

        public TimeSeriesRepository(InvestagerTimeSeriesContext context)
        {
            _context = context;
        }

        public async Task<TimeSeriesResponse> Get(string key)
        {
            var points = await _context.TimeSeriesPoints
                .Where(e => e.Key == key)
                .Select(e => new TimePointResponse { Time = e.Time, Value = e.Value })
                .ToListAsync();

            return new TimeSeriesResponse
            {
                Key = key,
                Points = points,
            };
        }

        public async Task InsertRange(IEnumerable<TimeSeriesPoint> timeSeriesPoints)
        {
            if (!timeSeriesPoints.Any())
            {
                return;
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("INSERT INTO \"TimeSeriesPoint\"(\"Time\", \"Key\", \"Value\")");
            sqlBuilder.AppendLine("VALUES");

            var pointsArray = timeSeriesPoints.ToArray();
            for (var i = 0; i < pointsArray.Length; i++)
            {
                sqlBuilder.Append($"('{pointsArray[i].Time:yyyy-MM-dd HH:mm:ss.ffffff}', '{pointsArray[i].Key}', {pointsArray[i].Value})");

                if (i == pointsArray.Length - 1)
                {
                    sqlBuilder.AppendLine(";");
                }
                else
                {
                    sqlBuilder.AppendLine(",");
                }
            }

            await _context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString());
        }
    }
}
