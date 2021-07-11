using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Models;
using Investager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Services
{
    public class AlpacaDataCollectionServiceUnitTests
    {
        private readonly Mock<ILogger<AlpacaDataCollectionService>> _mockLogger = new Mock<ILogger<AlpacaDataCollectionService>>();
        private readonly Mock<IDataProviderService> _mockDataProviderService = new Mock<IDataProviderService>();
        private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
        private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();
        private readonly AlpacaSettings _alpacaSettings = new AlpacaSettings();
        private readonly List<Asset> _assets;

        private readonly AlpacaDataCollectionService _target;

        public AlpacaDataCollectionServiceUnitTests()
        {
            _assets = new List<Asset>
            {
                new Asset
                {
                    Id = 1,
                    Symbol = "ZM",
                    LastPriceUpdate = new DateTime(2020, 02, 02),
                },
                new Asset
                {
                    Id = 2,
                    Symbol = "SE",
                },
                new Asset
                {
                    Id = 3,
                    Symbol = "ROKU",
                    LastPriceUpdate = new DateTime(2021, 02, 02),
                },
            };

            var mockFactory = new Mock<IDataProviderServiceFactory>();
            mockFactory.Setup(e => e.CreateService(DataProviders.Alpaca)).Returns(_mockDataProviderService.Object);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(e => e.CreateScope()).Returns(mockScope.Object);
            var mockServiceProvider = new Mock<IServiceProvider>();
            _mockAssetRepository.Setup(e => e.GetAllTracked()).ReturnsAsync(_assets);
            _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);
            mockServiceProvider.Setup(e => e.GetService(typeof(IDataProviderServiceFactory))).Returns(mockFactory.Object);
            mockServiceProvider.Setup(e => e.GetService(typeof(ICoreUnitOfWork))).Returns(_mockCoreUnitOfWork.Object);
            mockScope.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            _target = new AlpacaDataCollectionService(
                _mockLogger.Object,
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
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Once);
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
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Once);
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
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Task_OrdersAssetsByLastDateModified_BeforeCallingService()
        {
            // Arrange
            _alpacaSettings.PeriodBetweenDataRequests = TimeSpan.FromMilliseconds(1000);

            // Act
            _target.Start();
            await Task.Delay(200);
            _target.Stop();

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Once);
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.Is<Asset>(e => e.Id == 2)), Times.Once);
        }

        [Fact]
        public async Task Task_CallsUpdateAgain_AfterWaitingBetweenCollections()
        {
            // Arrange
            _alpacaSettings.PeriodBetweenDataRequests = TimeSpan.FromMilliseconds(1);
            _alpacaSettings.PeriodBetweenDataRequestBathes = TimeSpan.FromMilliseconds(300);

            // Act
            _target.Start();
            await Task.Delay(500);
            _target.Stop();

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Exactly(6));
        }

        [Fact]
        public async Task Task_WhenOneUpdateThrows_ContinuesSequence()
        {
            // Arrange
            _alpacaSettings.PeriodBetweenDataRequests = TimeSpan.FromMilliseconds(1);
            _mockDataProviderService.Setup(e => e.UpdateTimeSeriesData(It.IsAny<Asset>())).Throws(new Exception("big boomer."));

            // Act
            _target.Start();
            await Task.Delay(500);
            _target.Stop();

            // Assert
            _mockDataProviderService.Verify(e => e.UpdateTimeSeriesData(It.IsAny<Asset>()), Times.Exactly(3));
        }
    }
}
