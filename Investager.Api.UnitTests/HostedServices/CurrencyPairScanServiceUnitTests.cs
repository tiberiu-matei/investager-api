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

public class CurrencyPairScanServiceUnitTests
{
    private const string FirstProvider = "MockProvider1";
    private const string SecondProvider = "MockProvider2";

    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
    private readonly List<Mock<ICurrencyPairDataService>> _mockCurrencyPairDataServices = new List<Mock<ICurrencyPairDataService>>();
    private readonly Mock<ICurrencyPairDataService> _mockFirstCurrencyPairDataService = new Mock<ICurrencyPairDataService>();
    private readonly Mock<ICurrencyPairDataService> _mockSecondCurrencyPairDataService = new Mock<ICurrencyPairDataService>();
    private readonly Mock<ICurrencyService> _mockCurrencyService = new Mock<ICurrencyService>();
    private readonly Mock<IGenericRepository<Currency>> _mockCurrencyRepository = new Mock<IGenericRepository<Currency>>();
    private readonly Mock<IGenericRepository<CurrencyPair>> _mockCurrencyPairRepository = new Mock<IGenericRepository<CurrencyPair>>();
    private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
    private readonly Mock<ITimeSeriesRepository> _mockTimeSeriesRepository = new Mock<ITimeSeriesRepository>();
    private readonly DataScanSettings _dataScanSettings = new DataScanSettings();
    private readonly Mock<ICache> _mockCache = new Mock<ICache>();

    private readonly CancellationTokenSource _cancellationTokenSource;

    private CurrencyPairScanService _target;

    public CurrencyPairScanServiceUnitTests()
    {
        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory.Setup(e => e.CreateScope()).Returns(mockScope.Object);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider
            .Setup(e => e.GetService(typeof(IEnumerable<ICurrencyPairDataService>)))
            .Returns(() => _mockCurrencyPairDataServices.Select(e => e.Object));

        mockServiceProvider.Setup(e => e.GetService(typeof(ICurrencyService))).Returns(_mockCurrencyService.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ICoreUnitOfWork))).Returns(_mockCoreUnitOfWork.Object);
        mockServiceProvider.Setup(e => e.GetService(typeof(ITimeSeriesRepository))).Returns(_mockTimeSeriesRepository.Object);

        mockScope.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockFirstCurrencyPairDataService
            .Setup(e => e.Provider)
            .Returns(FirstProvider);

        _mockCurrencyPairDataServices.Add(_mockFirstCurrencyPairDataService);

        _mockCoreUnitOfWork.Setup(e => e.Currencies).Returns(_mockCurrencyRepository.Object);
        _mockCoreUnitOfWork.Setup(e => e.CurrencyPairs).Returns(_mockCurrencyPairRepository.Object);

        _cancellationTokenSource = new CancellationTokenSource();

        CreateTarget();
    }

    [Fact]
    public async Task Ensure_StartAsync_DoesNotBlock()
    {
        // Arrange
        _mockCurrencyService
            .Setup(e => e.GetAll())
            .ReturnsAsync(new List<Currency>());

        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        // Act
        _cancellationTokenSource.Cancel();
        await _target.StartAsync(_cancellationTokenSource.Token);

        // Assert
        _mockCurrencyService.Verify(e => e.GetAll(), Times.Never);
        _mockCurrencyService.Verify(e => e.GetPairs(), Times.Never);

        _mockFirstCurrencyPairDataService
            .Verify(e => e.GetPairs(), Times.Never);
    }

    [Fact]
    public async Task RequestingCancellation_StopScans()
    {
        // Arrange
        _mockCurrencyService
            .Setup(e => e.GetAll())
            .ReturnsAsync(new List<Currency>());

        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        var scanIntervalMs = 200;
        _dataScanSettings.ScanInterval = TimeSpan.FromMilliseconds(scanIntervalMs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(scanIntervalMs + 100);
        _cancellationTokenSource.Cancel();
        await Task.Delay(scanIntervalMs * 2);

        // Assert
        _mockCurrencyService.Verify(e => e.GetAll(), Times.Exactly(2));
        _mockCurrencyService.Verify(e => e.GetPairs(), Times.Exactly(2));

        _mockFirstCurrencyPairDataService
            .Verify(e => e.GetPairs(), Times.Exactly(2));
    }

    [Fact]
    public async Task Scan_WhenNoPairs_AddsEntries()
    {
        // Arrange
        var currencies = new List<Currency>();

        _mockCurrencyService
            .Setup(e => e.GetAll())
            .ReturnsAsync(() => currencies);

        _mockCurrencyRepository
            .Setup(e => e.Add(It.IsAny<Currency>()))
            .Callback((Currency currency) =>
            {
                currency.Id = currencies.Count() + 1;
                currencies.Add(currency);
            });

        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        var scanPairs = new List<CurrencyPair>
            {
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "eth",
                        Name = "ethereum",
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "usd",
                        Name = "KKona Dollah",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "btc",
                        Name = "bitcoin",
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "usd",
                        Name = "KKona Dollah",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
            };

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(scanPairs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstCurrencyPairDataService.Verify(e => e.GetPairs(), Times.Once);

        _mockCurrencyRepository.Verify(e => e.Add(It.IsAny<Currency>()), Times.Exactly(3));
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "eth")), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "usd")), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "btc")), Times.Once);

        _mockCurrencyPairRepository.Verify(e => e.Add(It.IsAny<CurrencyPair>()), Times.Exactly(4));
        _mockCurrencyPairRepository.Verify(e => e.Add(It.Is<CurrencyPair>(x => x.Provider == FirstProvider)), Times.Exactly(4));

        _mockCurrencyPairRepository
            .Verify(e => e.Add(It.Is<CurrencyPair>(
                x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 1
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 2
                    && x.HasTimeData)), Times.Once);

        _mockCurrencyPairRepository
            .Verify(e => e.Add(It.Is<CurrencyPair>(
                x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 2
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 1
                    && !x.HasTimeData)), Times.Once);

        _mockCurrencyPairRepository
            .Verify(e => e.Add(It.Is<CurrencyPair>(
                x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 3
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 2
                    && x.HasTimeData)), Times.Once);

        _mockCurrencyPairRepository
            .Verify(e => e.Add(It.Is<CurrencyPair>(
                x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 2
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 3
                    && !x.HasTimeData)), Times.Once);

        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(4));

        _mockCache.Verify(e => e.Clear(CacheKeys.CurrencyPairs), Times.Exactly(4));
        _mockCache.Verify(e => e.Clear(CacheKeys.Currencies), Times.Exactly(4));
    }

    [Fact]
    public async Task Scan_WhenPairsExist_OnlyAddsNewEntries()
    {
        // Arrange
        var eth = new Currency
        {
            Id = 104,
            Code = "eth",
            Name = "ethereum",
            Type = CurrencyType.Crypto,
        };

        var usd = new Currency
        {
            Id = 385,
            Code = "usd",
            Name = "KKona Dollah",
            Type = CurrencyType.Fiat,
        };

        var currencies = new List<Currency> { eth, usd };

        _mockCurrencyService
            .Setup(e => e.GetAll())
            .ReturnsAsync(() => currencies);

        _mockCurrencyRepository
            .Setup(e => e.Add(It.IsAny<Currency>()))
            .Callback((Currency currency) =>
            {
                currency.Id = currencies.Count() + 1;
                currencies.Add(currency);
            });

        var existingPairs = new List<CurrencyPair>
            {
                new CurrencyPair
                {
                    FirstCurrency = eth,
                    FirstCurrencyId = eth.Id,
                    SecondCurrency = usd,
                    SecondCurrencyId = usd.Id,
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = usd,
                    FirstCurrencyId = usd.Id,
                    SecondCurrency = eth,
                    SecondCurrencyId = eth.Id,
                    HasTimeData = false,
                    Provider = FirstProvider,
                }
            };

        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(existingPairs);

        var scanPairs = new List<CurrencyPair>
            {
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "eth",
                        Name = "ethereum",
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "usd",
                        Name = "KKona Dollah",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "btc",
                        Name = "bitcoin",
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "usd",
                        Name = "KKona Dollah",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "gbp",
                        Name = "Pound Sterling",
                        Type = CurrencyType.Fiat,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "eur",
                        Name = "Euro",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
            };

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(scanPairs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(300);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstCurrencyPairDataService.Verify(e => e.GetPairs(), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.IsAny<Currency>()), Times.Exactly(3));
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "btc")), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "gbp")), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.Is<Currency>(x => x.Code == "eur")), Times.Once);

        _mockCurrencyPairRepository.Verify(e => e.Add(It.IsAny<CurrencyPair>()), Times.Exactly(4));

        _mockCurrencyPairRepository
            .Verify(
                e => e.Add(It.Is<CurrencyPair>(x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 3
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 385
                    && x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCurrencyPairRepository
            .Verify(
                e => e.Add(It.Is<CurrencyPair>(x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 385
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 3
                    && !x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCurrencyPairRepository
            .Verify(
                e => e.Add(It.Is<CurrencyPair>(x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 4
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 5
                    && x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCurrencyPairRepository
            .Verify(
                e => e.Add(It.Is<CurrencyPair>(x => x.FirstCurrency == null
                    && x.FirstCurrencyId == 5
                    && x.SecondCurrency == null
                    && x.SecondCurrencyId == 4
                    && !x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Exactly(4));

        _mockCache.Verify(e => e.Clear(CacheKeys.CurrencyPairs), Times.Exactly(4));
        _mockCache.Verify(e => e.Clear(CacheKeys.Currencies), Times.Exactly(4));
    }

    [Fact]
    public async Task Scan_WhenMoreExistingPairs_RemovesOldEntries()
    {
        // Arrange
        var eth = new Currency
        {
            Id = 104,
            Code = "eth",
            Name = "ethereum",
            Type = CurrencyType.Crypto,
        };

        var btc = new Currency
        {
            Id = 601,
            Code = "btc",
            Name = "bitcoin",
            Type = CurrencyType.Crypto,
        };

        var usd = new Currency
        {
            Id = 385,
            Code = "usd",
            Name = "KKona Dollah",
            Type = CurrencyType.Fiat,
        };

        _mockCurrencyService
            .Setup(e => e.GetAll())
            .ReturnsAsync(new List<Currency> { eth, btc, usd });

        var existingPairs = new List<CurrencyPair>
            {
                new CurrencyPair
                {
                    FirstCurrency = eth,
                    FirstCurrencyId = eth.Id,
                    SecondCurrency = usd,
                    SecondCurrencyId = usd.Id,
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = usd,
                    FirstCurrencyId = usd.Id,
                    SecondCurrency = eth,
                    SecondCurrencyId = eth.Id,
                    HasTimeData = false,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = btc,
                    FirstCurrencyId = btc.Id,
                    SecondCurrency = usd,
                    SecondCurrencyId = usd.Id,
                    HasTimeData = true,
                    Provider = FirstProvider,
                },
                new CurrencyPair
                {
                    FirstCurrency = usd,
                    FirstCurrencyId = usd.Id,
                    SecondCurrency = btc,
                    SecondCurrencyId = btc.Id,
                    HasTimeData = false,
                    Provider = FirstProvider,
                },
            };

        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(existingPairs);

        var scanPairs = new List<CurrencyPair>
            {
                new CurrencyPair
                {
                    FirstCurrency = new Currency
                    {
                        Code = "eth",
                        Name = "ethereum",
                        Type = CurrencyType.Crypto,
                    },
                    SecondCurrency = new Currency
                    {
                        Code = "usd",
                        Name = "KKona Dollah",
                        Type = CurrencyType.Fiat,
                    },
                    HasTimeData = true,
                    Provider = FirstProvider,
                }
            };

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(scanPairs);

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockFirstCurrencyPairDataService.Verify(e => e.GetPairs(), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.IsAny<Currency>()), Times.Never);
        _mockCurrencyPairRepository.Verify(e => e.Add(It.IsAny<CurrencyPair>()), Times.Never);
        _mockCurrencyRepository.Verify(e => e.Delete(It.IsAny<Currency>()), Times.Never);
        _mockCurrencyPairRepository.Verify(e => e.Delete(It.IsAny<CurrencyPair>()), Times.Exactly(2));

        _mockCurrencyPairRepository
            .Verify(
                e => e.Delete(It.Is<CurrencyPair>(x => x.FirstCurrencyId == 601
                    && x.FirstCurrency != null
                    && x.SecondCurrencyId == 385
                    && x.SecondCurrency != null
                    && x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCurrencyPairRepository
            .Verify(
                e => e.Delete(It.Is<CurrencyPair>(x => x.FirstCurrencyId == 385
                    && x.FirstCurrency != null
                    && x.SecondCurrencyId == 601
                    && x.SecondCurrency != null
                    && !x.HasTimeData
                    && x.Provider == FirstProvider)),
                Times.Once);

        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);

        _mockCache.Verify(e => e.Clear(CacheKeys.CurrencyPairs), Times.Once);
        _mockCache.Verify(e => e.Clear(CacheKeys.Currencies), Times.Once);
    }

    [Fact]
    public async Task Scan_WhenMultipleProvidersExist_RunsForEach()
    {
        // Arrange
        _mockCurrencyService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        _mockFirstCurrencyPairDataService
            .Setup(e => e.GetPairs())
            .ReturnsAsync(new List<CurrencyPair>());

        _mockSecondCurrencyPairDataService
            .Setup(e => e.Provider)
            .Returns(SecondProvider);

        _mockCurrencyPairDataServices.Add(_mockSecondCurrencyPairDataService);
        CreateTarget();

        // Act
        await _target.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);
        _cancellationTokenSource.Cancel();

        // Assert
        _mockCurrencyService.Verify(e => e.GetPairs(), Times.Exactly(2));
        _mockFirstCurrencyPairDataService.Verify(e => e.GetPairs(), Times.Once);
        _mockSecondCurrencyPairDataService.Verify(e => e.GetPairs(), Times.Once);
        _mockCurrencyRepository.Verify(e => e.Add(It.IsAny<Currency>()), Times.Never);
        _mockCurrencyPairRepository.Verify(e => e.Add(It.IsAny<CurrencyPair>()), Times.Never);
        _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
        _mockCache.Verify(e => e.Clear(It.IsAny<string>()), Times.Never);
    }

    private void CreateTarget()
    {
        _target = new CurrencyPairScanService(
            _mockServiceScopeFactory.Object,
            _dataScanSettings,
            _mockCache.Object);
    }
}
