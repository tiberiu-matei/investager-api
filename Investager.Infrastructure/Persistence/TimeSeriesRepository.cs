using Investager.Core.Dtos;
using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence;

public class TimeSeriesRepository : ITimeSeriesRepository
{
    private readonly InvestagerTimeSeriesContext _context;
    private readonly IDbContextFactory<InvestagerTimeSeriesContext> _contextFactory;

    public TimeSeriesRepository(InvestagerTimeSeriesContext context, IDbContextFactory<InvestagerTimeSeriesContext> contextFactory)
    {
        _context = context;
        _contextFactory = contextFactory;
    }

    public async Task<TimeSeriesResponse> Get(string key)
    {
        using var readContext = _contextFactory.CreateDbContext();

        var points = await readContext.TimeSeriesPoints
            .Where(e => e.Key == key)
            .Select(e => new TimePointResponse { Time = e.Time, Value = e.Value })
            .ToListAsync();

        return new TimeSeriesResponse
        {
            Key = key,
            Points = points.OrderByDescending(e => e.Time).ToList(),
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

    public async Task DeleteSeries(string key)
    {
        var sql = $"DELETE FROM \"TimeSeriesPoint\" WHERE \"Key\" = '{key}'";
        using var context = _contextFactory.CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
