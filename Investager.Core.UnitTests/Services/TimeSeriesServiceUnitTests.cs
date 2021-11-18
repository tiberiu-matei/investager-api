using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services;

public class TimeSeriesServiceUnitTests
{
    private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
    private readonly Mock<ICache> _mockCache = new Mock<ICache>();
    private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();

    private readonly TimeSeriesService _target;

    public TimeSeriesServiceUnitTests()
    {
        _target = new TimeSeriesService(
            _mockTimeSeriesRepository.Object,
            _mockCache.Object,
            _mockTimeHelper.Object);
    }

    [Fact]
    public async Task Get_ReturnsCacheData()
    {
        // Assert
        var key = "NASDAQ:ZM";

        var timeSeries = new TimeSeriesSummary
        {
            Key = key,
            Points = new List<TimePointResponse>
                {
                    new TimePointResponse { Time = new DateTime(2021, 02, 02), Value = 103.5f },
                },
            GainLoss = new GainLossResponse
            {
                Last2Weeks = 3.01f,
            },
        };

        _mockCache.Setup(e => e.Get(It.IsAny<string>(), It.IsAny<Func<Task<TimeSeriesSummary>>>())).ReturnsAsync(timeSeries);

        // Act
        var response = await _target.Get(key);

        // Assert
        response.Should().Be(timeSeries);
        _mockCache.Verify(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesSummary>>>()), Times.Once);
    }

    [Fact]
    public async Task Get_CalculatesGainLossDataCorrectly()
    {
        // Assert
        var key = "NASDAQ:ZM";

        var utcNow = new DateTime(2021, 04, 20, 13, 37, 00, DateTimeKind.Utc);
        _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(utcNow);

        var timeSeriesResponse = new TimeSeriesResponse
        {
            Key = key,
            Points = new List<TimePointResponse>
                {
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 20, 14, 00, 00, DateTimeKind.Utc),
                        Value = 375.101f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 385.101f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 20, 12, 00, 00, DateTimeKind.Utc),
                        Value = 390.104f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 16, 13, 37, 00, DateTimeKind.Utc),
                        Value = 311.96f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 13, 13, 00, 00, DateTimeKind.Utc),
                        Value = 611.91f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 06, 13, 00, 00, DateTimeKind.Utc),
                        Value = 301.196f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 01, 13, 00, 00, DateTimeKind.Utc),
                        Value = 313.99f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 03, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 222.91f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 01, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 285.131f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2020, 04, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 232.15f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2019, 04, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 212.15f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2018, 04, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 401.15f,
                    },
                },
        };

        _mockTimeSeriesRepository
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(timeSeriesResponse);

        Func<Task<TimeSeriesSummary>> getDataFunc = null;

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<Func<Task<TimeSeriesSummary>>>()))
            .ReturnsAsync(new TimeSeriesSummary())
            .Callback((string key, Func<Task<TimeSeriesSummary>> func) => getDataFunc = func);

        // Act
        await _target.Get(key);
        var response = await getDataFunc.Invoke();

        // Assert
        _mockCache.Verify(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesSummary>>>()), Times.Once);
        _mockTimeSeriesRepository.Verify(e => e.Get(key), Times.Once);
        response.Points.Should().BeEquivalentTo(timeSeriesResponse.Points);
        response.Key.Should().Be(key);
        response.GainLoss.Last3Days.Should().Be(23.445642f);
        response.GainLoss.LastWeek.Should().Be(-37.065742f);
        response.GainLoss.Last2Weeks.Should().Be(27.8572731f);
        response.GainLoss.LastMonth.Should().Be(72.76076f);
        response.GainLoss.Last3Months.Should().Be(35.0610771f);
        response.GainLoss.LastYear.Should().Be(65.88457f);
        response.GainLoss.Last3Years.Should().Be(-4.000743f);
    }

    [Fact]
    public async Task Get_WhenNoPoints_ReturnsEmptyGainLoss()
    {
        // Assert
        var key = "NASDAQ:ZM";

        var utcNow = new DateTime(2021, 04, 20, 13, 37, 00, DateTimeKind.Utc);
        _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(utcNow);

        var timeSeriesResponse = new TimeSeriesResponse
        {
            Key = key,
            Points = new List<TimePointResponse>(),
        };

        _mockTimeSeriesRepository
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(timeSeriesResponse);

        Func<Task<TimeSeriesSummary>> getDataFunc = null;

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<Func<Task<TimeSeriesSummary>>>()))
            .ReturnsAsync(new TimeSeriesSummary())
            .Callback((string key, Func<Task<TimeSeriesSummary>> func) => getDataFunc = func);

        // Act
        await _target.Get(key);
        var response = await getDataFunc.Invoke();

        // Assert
        _mockCache.Verify(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesSummary>>>()), Times.Once);
        _mockTimeSeriesRepository.Verify(e => e.Get(key), Times.Once);
        response.Points.Should().BeEquivalentTo(timeSeriesResponse.Points);
        response.Key.Should().Be(key);
        response.GainLoss.Last3Days.Should().BeNull();
        response.GainLoss.LastWeek.Should().BeNull();
        response.GainLoss.Last2Weeks.Should().BeNull();
        response.GainLoss.LastMonth.Should().BeNull();
        response.GainLoss.Last3Months.Should().BeNull();
        response.GainLoss.LastYear.Should().BeNull();
        response.GainLoss.Last3Years.Should().BeNull();
    }

    [Fact]
    public async Task Get_WhenPointsTooFar_ReturnsEmptyGainLoss()
    {
        // Assert
        var key = "NASDAQ:ZM";

        var utcNow = new DateTime(2021, 04, 20, 13, 37, 00, DateTimeKind.Utc);
        _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(utcNow);

        var timeSeriesResponse = new TimeSeriesResponse
        {
            Key = key,
            Points = new List<TimePointResponse>
                {
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 04, 13, 15, 00, 00, DateTimeKind.Utc),
                        Value = 375.101f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 03, 21, 13, 00, 00, DateTimeKind.Utc),
                        Value = 385.101f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 03, 14, 12, 00, 00, DateTimeKind.Utc),
                        Value = 390.104f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2021, 02, 20, 13, 37, 00, DateTimeKind.Utc),
                        Value = 311.96f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2020, 07, 13, 13, 00, 00, DateTimeKind.Utc),
                        Value = 611.91f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2020, 01, 06, 13, 00, 00, DateTimeKind.Utc),
                        Value = 301.196f,
                    },
                    new TimePointResponse
                    {
                        Time = new DateTime(2017, 04, 20, 13, 00, 00, DateTimeKind.Utc),
                        Value = 401.15f,
                    },
                },
        };

        _mockTimeSeriesRepository
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(timeSeriesResponse);

        Func<Task<TimeSeriesSummary>> getDataFunc = null;

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<Func<Task<TimeSeriesSummary>>>()))
            .ReturnsAsync(new TimeSeriesSummary())
            .Callback((string key, Func<Task<TimeSeriesSummary>> func) => getDataFunc = func);

        // Act
        await _target.Get(key);
        var response = await getDataFunc.Invoke();

        // Assert
        _mockCache.Verify(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesSummary>>>()), Times.Once);
        _mockTimeSeriesRepository.Verify(e => e.Get(key), Times.Once);
        response.Points.Should().BeEquivalentTo(timeSeriesResponse.Points);
        response.Key.Should().Be(key);
        response.GainLoss.Last3Days.Should().BeNull();
        response.GainLoss.LastWeek.Should().BeNull();
        response.GainLoss.Last2Weeks.Should().BeNull();
        response.GainLoss.LastMonth.Should().BeNull();
        response.GainLoss.Last3Months.Should().BeNull();
        response.GainLoss.LastYear.Should().BeNull();
        response.GainLoss.Last3Years.Should().BeNull();
    }
}
