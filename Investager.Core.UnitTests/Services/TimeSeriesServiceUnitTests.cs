using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services
{
    public class TimeSeriesServiceUnitTests
    {
        private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();

        private readonly TimeSeriesService _target;

        public TimeSeriesServiceUnitTests()
        {
            _target = new TimeSeriesService(_mockTimeSeriesRepository.Object, _mockCache.Object);
        }

        [Fact]
        public async Task Get_ReturnsCacheData()
        {
            // Assert
            var key = "NASDAQ:ZM";

            var timeSeries = new TimeSeriesResponse
            {
                Key = key,
                Points = new List<TimePointResponse>
                {
                    new TimePointResponse { Time = new DateTime(2021, 02, 02), Value = 103.5f },
                },
            };

            _mockCache.Setup(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesResponse>>>())).ReturnsAsync(timeSeries);

            // Act
            var response = await _target.Get(key);

            // Assert
            response.Should().Be(timeSeries);
            _mockCache.Verify(e => e.Get(key, It.IsAny<Func<Task<TimeSeriesResponse>>>()), Times.Once);
        }
    }
}
