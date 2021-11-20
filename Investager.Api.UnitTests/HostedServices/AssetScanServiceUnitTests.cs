using Investager.Api.HostedServices;
using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Investager.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.HostedServices;

public class AssetScanServiceUnitTests
{
    private const string FirstProvider = "MockProvider1";
    private const string SecondProvider = "MockProvider2";

    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
    private readonly List<Mock<IAssetDataService>> _mockAssetDataServices = new List<Mock<IAssetDataService>>();
    private readonly Mock<IAssetDataService> _mockFirstAssetDataService = new Mock<IAssetDataService>();
    private readonly Mock<IAssetDataService> _mockSecondAssetDataService = new Mock<IAssetDataService>();
    private readonly Mock<IAssetService> _mockAssetService = new Mock<IAssetService>();
    private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();
    private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
    private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
    private readonly DataScanSettings _dataScanSettings = new DataScanSettings();
    private readonly Mock<ICache> _mockCache = new Mock<ICache>();

    private readonly CancellationTokenSource _cancellationTokenSource;

    private AssetScanService _target;

    public AssetScanServiceUnitTests()
    {
        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory.Setup(e => e.CreateScope()).Returns(mockScope.Object);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider
            .Setup(e => e.GetService(typeof(IEnumerable<IAssetDataService>)))
            .Returns(() => _mockAssetDataServices.Select(e => e.Object));

        mockServiceProvider.Setup(e => e.GetService(typeof(IAssetService))).Returns(_mockAssetService.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ICoreUnitOfWork))).Returns(_mockCoreUnitOfWork.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ITimeSeriesRepository))).Returns(_mockTimeSeriesRepository.Object);

        mockScope.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockFirstAssetDataService
            .Setup(e => e.Provider)
            .Returns(FirstProvider);

        _mockAssetDataServices.Add(_mockFirstAssetDataService);

        _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);

        _cancellationTokenSource = new CancellationTokenSource();

        CreateTarget();
    }

    [Fact]
    public async Task Ensure_StartAsync_DoesNotBlock()
    {
        // Arrange
        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset>());

        // Act
        _cancellationTokenSource.Cancel();
        await _target.StartAsync(_cancellationTokenSource.Token);

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Never);

        _mockFirstAssetDataService
            .Verify(e => e.GetAssets(), Times.Never);
    }

    [Fact]
    public async Task RequestingCancellation_StopScans()
    {
        // Arrange
        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset>());

        var scanIntervalMs = 300;
        _dataScanSettings.ScanInterval = TimeSpan.FromMilliseconds(scanIntervalMs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(scanIntervalMs + 100);
        _cancellationTokenSource.Cancel();
        await Task.Delay(scanIntervalMs * 2);

        // Assert
        _mockFirstAssetDataService
            .Verify(e => e.GetAssets(), Times.Exactly(2));
    }

    [Fact]
    public async Task Scan_WhenNoAssets_AddsEntries()
    {
        // Arrange
        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset>());

        var scanAssets = new List<Asset>
            {
                new Asset
                {
                    Name = "Zoom",
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
                new Asset
                {
                    Name = "Sea Limited",
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                }
            };

        _mockFirstAssetDataService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(scanAssets);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstAssetDataService.Verify(e => e.GetAssets(), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.IsAny<Asset>()), Times.Exactly(2));
        _mockAssetRepository.Verify(e => e.Add(It.Is<Asset>(x => x.Symbol == "ZM")), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.Is<Asset>(x => x.Symbol == "SE")), Times.Once);
        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);

        _mockCache.Verify(e => e.Clear(CacheKeys.AssetDtos), Times.Once);
        _mockCache.Verify(e => e.Clear(CacheKeys.Assets), Times.Once);
    }

    [Fact]
    public async Task Scan_WhenAssetsExist_OnlyAddsNewEntries()
    {
        // Arrange
        var existingAssets = new List<Asset>
            {
                new Asset
                {
                    Name = "Zoom",
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = SecondProvider,
                },
                new Asset
                {
                    Name = "UiPath",
                    Symbol = "PATH",
                    Exchange = "NYSE",
                    Currency = "USD",
                    Provider = SecondProvider,
                },
            };

        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(existingAssets);

        var scanAssets = new List<Asset>
            {
                new Asset
                {
                    Name = "Zoom",
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
                new Asset
                {
                    Name = "Sea Limited",
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
            };

        _mockFirstAssetDataService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(scanAssets);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstAssetDataService.Verify(e => e.GetAssets(), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.IsAny<Asset>()), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.Is<Asset>(x => x.Symbol == "SE")), Times.Once);
        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);

        _mockCache.Verify(e => e.Clear(CacheKeys.AssetDtos), Times.Once);
        _mockCache.Verify(e => e.Clear(CacheKeys.Assets), Times.Once);
    }

    [Fact]
    public async Task Scan_WhenMoreExistingAssets_RemovesOldEntries()
    {
        // Arrange
        var existingAssets = new List<Asset>
            {
                new Asset
                {
                    Name = "UiPath",
                    Symbol = "PATH",
                    Exchange = "NYSE",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
                new Asset
                {
                    Name = "Zoom",
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
            };

        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(existingAssets);

        var scanAssets = new List<Asset>
            {
                new Asset
                {
                    Name = "Zoom",
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
                new Asset
                {
                    Name = "Sea Limited",
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Currency = "USD",
                    Provider = FirstProvider,
                },
            };

        _mockFirstAssetDataService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(scanAssets);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstAssetDataService.Verify(e => e.GetAssets(), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.IsAny<Asset>()), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.Is<Asset>(x => x.Symbol == "SE")), Times.Once);

        _mockAssetRepository.Verify(e => e.Delete(It.IsAny<Asset>()), Times.Once);
        _mockAssetRepository.Verify(e => e.Delete(existingAssets.First()), Times.Once);
        _mockTimeSeriesRepository.Verify(e => e.DeleteSeries("NYSE:PATH"), Times.Once);

        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);

        _mockCache.Verify(e => e.Clear("NYSE:PATH"), Times.Once);
        _mockCache.Verify(e => e.Clear(CacheKeys.AssetDtos), Times.Once);
        _mockCache.Verify(e => e.Clear(CacheKeys.Assets), Times.Once);
    }

    [Fact]
    public async Task Scan_WhenMultipleProvidersExist_RunsForEach()
    {
        // Arrange
        _mockAssetService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset>());

        _mockFirstAssetDataService
            .Setup(e => e.GetAssets())
            .ReturnsAsync(new List<Asset>());

        _mockSecondAssetDataService
            .Setup(e => e.Provider)
            .Returns(SecondProvider);

        _mockAssetDataServices.Add(_mockSecondAssetDataService);
        CreateTarget();

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockAssetService.Verify(e => e.GetAssets(), Times.Exactly(2));
        _mockFirstAssetDataService.Verify(e => e.GetAssets(), Times.Once);
        _mockSecondAssetDataService.Verify(e => e.GetAssets(), Times.Once);
        _mockAssetRepository.Verify(e => e.Add(It.IsAny<Asset>()), Times.Never);
        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        _mockCache.Verify(e => e.Clear(It.IsAny<string>()), Times.Never);
    }

    private void CreateTarget()
    {
        _target = new AssetScanService(
            _mockServiceScopeFactory.Object,
            _dataScanSettings,
            _mockCache.Object);
    }
}
