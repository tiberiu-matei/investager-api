using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers;

public class TimeSeriesControllerUnitTests
{
    private readonly Mock<ITimeSeriesService> _mockTimeSeriesService = new Mock<ITimeSeriesService>();

    private readonly TimeSeriesController _target;

    public TimeSeriesControllerUnitTests()
    {
        _target = new TimeSeriesController(_mockTimeSeriesService.Object);
    }

    [Fact]
    public async Task Get_ReturnsDataFromService()
    {
        // Arrange
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
                Last2Weeks = 35.3f,
                LastYear = -88.91f,
            },
        };

        _mockTimeSeriesService.Setup(e => e.Get(key)).ReturnsAsync(timeSeries);

        // Act
        var response = await _target.Get(key);

        // Assert
        response.Value.Should().BeEquivalentTo(timeSeries.Points);
    }
}
