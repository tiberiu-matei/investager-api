using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class TimeSeriesService : ITimeSeriesService
    {
        private readonly ITimeSeriesRepository _timeSeriesRepository;
        private readonly ICache _cache;
        private readonly ITimeHelper _timeHelper;

        public TimeSeriesService(ITimeSeriesRepository timeSeriesRepository, ICache cache, ITimeHelper timeHelper)
        {
            _timeSeriesRepository = timeSeriesRepository;
            _cache = cache;
            _timeHelper = timeHelper;
        }

        public Task<TimeSeriesSummary> Get(string key)
        {
            return _cache.Get(key, async () => await GetData(key));
        }

        private async Task<TimeSeriesSummary> GetData(string key)
        {
            var timeSeries = await _timeSeriesRepository.Get(key);

            var now = _timeHelper.GetUtcNow();

            var threeDaysAgo = now.AddDays(-3);
            var oneWeekAgo = now.AddDays(-7);
            var twoWeeksAgo = now.AddDays(-14);
            var oneMonthAgo = now.AddMonths(-1);
            var threeMonthsAgo = now.AddMonths(-3);
            var oneYearAgo = now.AddYears(-1);
            var threeYearsAgo = now.AddYears(-3);

            var pointsArray = timeSeries.Points.ToArray();

            var nowPoint = GetClosestPoint(pointsArray, now);
            var gainLoss = new GainLossResponse
            {
                Last3Days = GetGainLoss(pointsArray, threeDaysAgo, nowPoint),
                LastWeek = GetGainLoss(pointsArray, oneWeekAgo, nowPoint),
                Last2Weeks = GetGainLoss(pointsArray, twoWeeksAgo, nowPoint),
                LastMonth = GetGainLoss(pointsArray, oneMonthAgo, nowPoint),
                Last3Months = GetGainLoss(pointsArray, threeMonthsAgo, nowPoint),
                LastYear = GetGainLoss(pointsArray, oneYearAgo, nowPoint),
                Last3Years = GetGainLoss(pointsArray, threeYearsAgo, nowPoint),
            };

            return new TimeSeriesSummary
            {
                Key = timeSeries.Key,
                Points = timeSeries.Points,
                GainLoss = gainLoss,
            };
        }

        private float? GetGainLoss(TimePointResponse[] points, DateTime from, TimePointResponse toPoint)
        {
            var fromPoint = GetClosestPoint(points, from);

            return fromPoint != null && toPoint != null
                ? (toPoint.Value - fromPoint.Value) / fromPoint.Value * 100
                : null;
        }

        private TimePointResponse GetClosestPoint(TimePointResponse[] points, DateTime time)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var pointTime = points[i].Time;

                if (pointTime <= time)
                {
                    return time - pointTime < TimeSpan.FromDays(5) ? points[i] : null;
                }
            }

            return null;
        }
    }
}
