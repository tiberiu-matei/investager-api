using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Services
{
    public class AlpacaDataCollectionServiceUnitTests
    {
        private readonly Mock<IDataProviderService> _mockDataProviderService = new Mock<IDataProviderService>();
        private readonly AlpacaSettings _alpacaSettings = new AlpacaSettings();

        private readonly AlpacaDataCollectionService _target;

        public AlpacaDataCollectionServiceUnitTests()
        {
            var mockFactory = new Mock<IDataProviderServiceFactory>();
            mockFactory.Setup(e => e.CreateService(DataProviders.Alpaca)).Returns(_mockDataProviderService.Object);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(e => e.CreateScope()).Returns(mockScope.Object);
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(e => e.GetService(typeof(IDataProviderServiceFactory))).Returns(mockFactory.Object);
            mockScope.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            _target = new AlpacaDataCollectionService(
                mockServiceScopeFactory.Object,
                _alpacaSettings);
        }

        [Fact]
        public async Task Start_CalledTwice_DoesNotAddASecondTask()
        {
            // Act
            _target.Start();
            _target.Start();

            await Task.Delay(200);
            _target.Stop();

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(), Times.Once);
        }

        [Fact]
        public async Task Stop_FinishesTheTask()
        {
            // Arrange
            _alpacaSettings.PeriodBetweenDataRequests = TimeSpan.FromMilliseconds(400);
            _target.Start();
            await Task.Delay(100);

            // Act
            _target.Stop();
            await Task.Delay(500);

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(), Times.Once);
        }

        [Fact]
        public async Task Task_RunsAtExpectedIntervals()
        {
            // Arrange
            _alpacaSettings.PeriodBetweenDataRequests = TimeSpan.FromMilliseconds(400);

            // Act
            _target.Start();
            await Task.Delay(600);
            _target.Stop();

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(), Times.Exactly(2));
        }
    }
}
