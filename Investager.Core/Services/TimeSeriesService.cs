using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
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
            return _cache.Get(key, async () =>
            {
                var response = await _timeSeriesRepository.Get(key);

                return GetData(response);
            });
        }

        public Task<TimeSeriesSummary> Get(CurrencyPair currencyPair)
        {
            var key = $"{currencyPair.FirstCurrency.Code}/{currencyPair.SecondCurrency.Code}";
            return _cache.Get(key, async () =>
            {
                if (currencyPair.HasTimeData)
                {
                    var response = await _timeSeriesRepository.Get(key);

                    return GetData(response);
                }
                else
                {
                    var dataKey = $"{currencyPair.SecondCurrency.Code}/{currencyPair.FirstCurrency.Code}";
                    var response = await _timeSeriesRepository.Get(dataKey);
                    foreach (var point in response.Points)
                    {
                        point.Value = 1 / point.Value;
                    }

                    return GetData(response);
                }
            });
        }

        private TimeSeriesSummary GetData(TimeSeriesResponse timeSeriesResponse)
        {
            var gainLoss = GetGainLossResponse(timeSeriesResponse.Points);

            return new TimeSeriesSummary
            {
                Key = timeSeriesResponse.Key,
                Points = timeSeriesResponse.Points,
                GainLoss = gainLoss,
            };
        }

        private GainLossResponse GetGainLossResponse(ICollection<TimePointResponse> points)
        {
            var now = _timeHelper.GetUtcNow();

            var threeDaysAgo = now.AddDays(-3);
            var oneWeekAgo = now.AddDays(-7);
            var twoWeeksAgo = now.AddDays(-14);
            var oneMonthAgo = now.AddMonths(-1);
            var threeMonthsAgo = now.AddMonths(-3);
            var oneYearAgo = now.AddYears(-1);
            var threeYearsAgo = now.AddYears(-3);

            var pointsArray = points.ToArray();

            var nowPoint = GetClosestPoint(pointsArray, now);
            return new GainLossResponse
            {
                Last3Days = GetGainLoss(pointsArray, threeDaysAgo, nowPoint),
                LastWeek = GetGainLoss(pointsArray, oneWeekAgo, nowPoint),
                Last2Weeks = GetGainLoss(pointsArray, twoWeeksAgo, nowPoint),
                LastMonth = GetGainLoss(pointsArray, oneMonthAgo, nowPoint),
                Last3Months = GetGainLoss(pointsArray, threeMonthsAgo, nowPoint),
                LastYear = GetGainLoss(pointsArray, oneYearAgo, nowPoint),
                Last3Years = GetGainLoss(pointsArray, threeYearsAgo, nowPoint),
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
