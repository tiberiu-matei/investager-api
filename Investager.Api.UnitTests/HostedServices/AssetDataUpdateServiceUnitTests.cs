using Investager.Api.HostedServices;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.HostedServices;

public class AssetDataUpdateServiceUnitTests
{
    private const int DataQueryIntervalMs = 200;
    private const string FirstProvider = "MockProvider1";
    private const string SecondProvider = "MockProvider2";

    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
    private readonly List<Mock<IAssetDataService>> _mockAssetDataServices = new List<Mock<IAssetDataService>>();
    private readonly Mock<IAssetDataService> _mockFirstAssetDataService = new Mock<IAssetDataService>();
    private readonly Mock<IAssetDataService> _mockSecondAssetDataService = new Mock<IAssetDataService>();
    private readonly Mock<IAssetService> _mockAssetService = new Mock<IAssetService>();
    private readonly Mock<ICache> _mockCache = new Mock<ICache>();
    private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
    private readonly Mock<ITimeSeriesService> _mockTimeSeriesService = new Mock<ITimeSeriesService>();
    private readonly DataUpdateSettings _dataUpdateSettings = new DataUpdateSettings();
    private readonly Mock<ILogger<AssetDataUpdateService>> _mockLogger = new Mock<ILogger<AssetDataUpdateService>>();

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly Asset _asset;

    private AssetDataUpdateService _target;

    public AssetDataUpdateServiceUnitTests()
    {
        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory.Setup(e => e.CreateScope()).Returns(mockScope.Object);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider
            .Setup(e => e.GetService(typeof(IEnumerable<IAssetDataService>)))
            .Returns(() => _mockAssetDataServices.Select(e => e.Object));

        mockServiceProvider.Setup(e => e.GetService(typeof(IAssetService))).Returns(_mockAssetService.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ITimeSeriesRepository))).Returns(_mockTimeSeriesRepository.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ITimeSeriesService))).Returns(_mockTimeSeriesService.Object);

        mockScope.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockFirstAssetDataService
            .Setup(e => e.Provider)
            .Returns(FirstProvider);

        _mockFirstAssetDataService
            .Setup(e => e.DataQueryInterval)
            .Returns(TimeSpan.FromMilliseconds(DataQueryIntervalMs));

        _mockAssetDataServices.Add(_mockFirstAssetDataService);

        _asset = new Asset
        {
            Id = 131,
            Name = "SEA Ltd",
            Symbol = "SE",
            Exchange = "NASDAQ",
            Currency = "USD",
            Provider = FirstProvider,
        };

        _mockAssetService.Setup(e => e.GetAssets()).ReturnsAsync(new List<Asset> { _asset });

        _cancellationTokenSource = new CancellationTokenSource();

        CreateTarget();
    }

    [Fact]
    public async Task Ensure_StartAsync_DoesNotBlock()
    {
        // Arrange
        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = new List<TimePointResponse>() });

        // Act
        _cancellationTokenSource.Cancel();
        await _target.StartAsync(_cancellationTokenSource.Token);

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Never);

        _mockFirstAssetDataService
            .Verify(e => e.GetRecentPoints(It.IsAny<UpdateAssetDataRequest>()), Times.Never);
    }

    [Fact]
    public async Task RequestingCancellation_StopUpdates()
    {
        // Arrange
        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = new List<TimePointResponse>() });

        var dataUpdateIntervalMs = 200;
        _dataUpdateSettings.UpdateInterval = TimeSpan.FromMilliseconds(dataUpdateIntervalMs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(DataQueryIntervalMs + dataUpdateIntervalMs + 100);
        _cancellationTokenSource.Cancel();
        await Task.Delay((DataQueryIntervalMs + dataUpdateIntervalMs) * 2);

        // Assert
        _mockFirstAssetDataService
            .Verify(e => e.GetRecentPoints(It.IsAny<UpdateAssetDataRequest>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Data_WhenNoneExisting_IsUpdatedCorrectly()
    {
        // Arrange
        var recentPoints = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(1985, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Key = "NASDAQ:SE",
                    Value = 13.37f,
                },
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Key = "NASDAQ:SE",
                    Value = 385.139f,
                },
            };

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = new List<TimePointResponse>() });

        _mockFirstAssetDataService
            .Setup(e => e.GetRecentPoints(It.IsAny<UpdateAssetDataRequest>()))
            .ReturnsAsync(recentPoints);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Exactly(2));
        _mockFirstAssetDataService.Verify(e => e.GetRecentPoints(
            It.Is<UpdateAssetDataRequest>(e => e.Key == "NASDAQ:SE"
                && e.Exchange == "NASDAQ"
                && e.Symbol == "SE"
                && e.LatestPointTime == null)),
            Times.Once);

        _mockTimeSeriesRepository.Verify(e => e.InsertRange(recentPoints), Times.Once);
    }

    [Fact]
    public async Task Data_WhenPointsExist_IsUpdatedCorrectly()
    {
        // Arrange
        var recentPoints = new List<TimeSeriesPoint>
            {
                new TimeSeriesPoint
                {
                    Time = new DateTime(2020, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Key = "NASDAQ:SE",
                    Value = 13.37f,
                },
                new TimeSeriesPoint
                {
                    Time = new DateTime(2021, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Key = "NASDAQ:SE",
                    Value = 385.139f,
                },
            };

        var existingPoints = new List<TimePointResponse>
            {
                new TimePointResponse
                {
                    Time = new DateTime(2019, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Value = 1.22f,
                },
                new TimePointResponse
                {
                    Time = new DateTime(2018, 10, 10, 13, 37, 00, DateTimeKind.Utc),
                    Value = 6.01f,
                },
            };

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = existingPoints });

        _mockFirstAssetDataService
            .Setup(e => e.GetRecentPoints(It.IsAny<UpdateAssetDataRequest>()))
            .ReturnsAsync(recentPoints);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Exactly(2));
        _mockFirstAssetDataService.Verify(e => e.GetRecentPoints(
            It.Is<UpdateAssetDataRequest>(e => e.Key == "NASDAQ:SE"
                && e.Exchange == "NASDAQ"
                && e.Symbol == "SE"
                && e.LatestPointTime == existingPoints.First().Time)),
            Times.Once);

        _mockTimeSeriesRepository.Verify(e => e.InsertRange(recentPoints), Times.Once);
    }

    [Fact]
    public async Task Data_WhenAssetIsDeletedDuringUpdate_DoesNotRetrievePoints()
    {
        // Arrange
        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = new List<TimePointResponse>() });

        _mockAssetService.SetupSequence(e => e.GetAssets())
            .ReturnsAsync(new List<Asset> { _asset })
            .ReturnsAsync(new List<Asset>());

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Exactly(2));
        _mockFirstAssetDataService.Verify(e => e.GetRecentPoints(It.IsAny<UpdateAssetDataRequest>()), Times.Never);
        _mockTimeSeriesRepository.Verify(e => e.InsertRange(It.IsAny<IEnumerable<TimeSeriesPoint>>()), Times.Never);
    }

    [Fact]
    public async Task MultipleServices_UpdateData_InParallel_UsingOwnTimers()
    {
        // Arrange
        var secondQueryIntervalMs = 500;
        _mockSecondAssetDataService.Setup(e => e.Provider).Returns(SecondProvider);
        _mockSecondAssetDataService.Setup(e => e.DataQueryInterval).Returns(TimeSpan.FromMilliseconds(secondQueryIntervalMs));
        _mockAssetDataServices.Add(_mockSecondAssetDataService);
        CreateTarget();

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { Points = new List<TimePointResponse>() });

        _dataUpdateSettings.UpdateInterval = TimeSpan.FromMilliseconds(1);

        var secondProviderAsset = new Asset
        {
            Id = 892,
            Name = "Zooom",
            Symbol = "ZM",
            Exchange = "NASDAQ",
            Currency = "USD",
            Provider = SecondProvider,
        };

        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset> { _asset, secondProviderAsset });

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(300);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Exactly(6));
        _mockSecondAssetDataService.Verify(e => e.GetRecentPoints(It.Is<UpdateAssetDataRequest>(e => e.Symbol == "ZM")), Times.Once);
        _mockFirstAssetDataService.Verify(e => e.GetRecentPoints(It.Is<UpdateAssetDataRequest>(e => e.Symbol == "SE")), Times.Exactly(2));
        _mockTimeSeriesRepository.Verify(e => e.InsertRange(It.IsAny<IEnumerable<TimeSeriesPoint>>()), Times.Never);
    }

    private void CreateTarget()
    {
        _target = new AssetDataUpdateService(
            _mockServiceScopeFactory.Object,
            _mockCache.Object,
            _dataUpdateSettings,
            _mockLogger.Object);
    }
}
