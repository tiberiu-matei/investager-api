using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
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
            var timeSeries = new TimeSeriesResponse
            {
                Key = key,
                Points = new List<TimePointResponse>
                {
                    new TimePointResponse { Time = new DateTime(2021, 02, 02), Value = 103.5f },
                },
            };

            _mockTimeSeriesService.Setup(e => e.Get(key)).ReturnsAsync(timeSeries);

            // Act
            var response = await _target.Get(key);

            // Assert
            var result = response as OkObjectResult;
            var value = result.Value as TimeSeriesResponse;
            result.StatusCode.Should().Be(200);
            value.Should().Be(timeSeries);
        }
    }
}
