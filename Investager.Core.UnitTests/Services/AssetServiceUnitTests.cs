using AutoMapper;
using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Core.UnitTests.Services;

public class AssetServiceUnitTests
{
    private readonly Mock<ICoreUnitOfWork> _mockCoreUnitOfWork = new Mock<ICoreUnitOfWork>();
    private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    private readonly Mock<ICache> _mockCache = new Mock<ICache>();
    private readonly Mock<ITimeSeriesService> _mockTimeSeriesService = new Mock<ITimeSeriesService>();
    private readonly Mock<IFuzzyMatch> _mockFuzzyMatch = new Mock<IFuzzyMatch>();

    private readonly Mock<IGenericRepository<Asset>> _mockAssetRepository = new Mock<IGenericRepository<Asset>>();
    private readonly Mock<IGenericRepository<WatchlistAsset>> _mockUserStarredAssetRepository = new Mock<IGenericRepository<WatchlistAsset>>();

    private readonly AssetService _target;

    public AssetServiceUnitTests()
    {
        _mockCoreUnitOfWork.Setup(e => e.Assets).Returns(_mockAssetRepository.Object);
        // _mockCoreUnitOfWork.Setup(e => e.UserStarredAssets).Returns(_mockUserStarredAssetRepository.Object);

        _target = new AssetService(
            _mockCoreUnitOfWork.Object,
            _mockMapper.Object,
            _mockCache.Object,
            _mockTimeSeriesService.Object,
            _mockFuzzyMatch.Object);
    }

    [Fact]
    public async Task Search_CachesAssetSummariesCorrectly()
    {
        // Arrange
        var assets = new List<Asset>
            {
                new Asset
                {
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new Asset
                {
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
            };

        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new AssetSummaryDto
                {
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
            };

        _mockMapper
            .Setup(e => e.Map<IEnumerable<AssetSummaryDto>>(assets))
            .Returns(summaries);

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = new GainLossResponse() });

        Func<Task<IEnumerable<AssetSummaryDto>>> getSummariesFunc = null;

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(new List<AssetSummaryDto>())
            .Callback((string key, TimeSpan ttl, Func<Task<IEnumerable<AssetSummaryDto>>> func) => getSummariesFunc = func);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<Asset>>>>()))
            .ReturnsAsync(assets);

        // Act
        await _target.Search("abc", 10);
        var response = await getSummariesFunc();

        // Assert
        response.Count().Should().Be(2);
        response.Should().BeEquivalentTo(summaries);
        _mockMapper.Verify(e => e.Map<IEnumerable<AssetSummaryDto>>(assets), Times.Once);
        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockCache.Verify(e => e.Get("Assets", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<Asset>>>>()), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoEntries_ReturnsEmptyList()
    {
        // Arrange
        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(new List<AssetSummaryDto>());

        // Act
        var response = await _target.Search("abc", 1);

        // Assert
        response.Assets.Count().Should().Be(0);
        response.MoreRecordsAvailable.Should().BeFalse();

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new AssetSummaryDto
                {
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
            };

        _mockFuzzyMatch
            .Setup(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(3);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(summaries);

        // Act
        var response = await _target.Search("abc", 1);

        // Assert
        response.Assets.Count().Should().Be(0);
        response.MoreRecordsAvailable.Should().BeFalse();

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
    }

    [Fact]
    public async Task Search_PrioritizesSymbolsByMatches()
    {
        // Arrange
        var searchText = "abc";
        var max = 2;

        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new AssetSummaryDto
                {
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
                new AssetSummaryDto
                {
                    Symbol = "PATH",
                    Exchange = "NYSE",
                    Name = "UIPath",
                },
            };

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Symbol))
            .Returns(1);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Name))
            .Returns(10);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Symbol))
            .Returns(2);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Name))
            .Returns(10);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Symbol))
            .Returns(0);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Name))
            .Returns(10);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(summaries);

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = new GainLossResponse() });

        // Act
        var response = await _target.Search(searchText, max);

        // Assert
        response.Assets.Count().Should().Be(2);
        response.Assets.ToArray()[0].Symbol.Should().Be("PATH");
        response.Assets.ToArray()[1].Symbol.Should().Be("Z");
        response.MoreRecordsAvailable.Should().BeTrue();

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(6));
    }

    [Fact]
    public async Task Search_PrioritizesNamesByMatches()
    {
        // Arrange
        var searchText = "abc";
        var max = 2;

        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new AssetSummaryDto
                {
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
                new AssetSummaryDto
                {
                    Symbol = "PATH",
                    Exchange = "NYSE",
                    Name = "UIPath",
                },
            };

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Symbol))
            .Returns(10);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Name))
            .Returns(1);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Symbol))
            .Returns(10);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Name))
            .Returns(2);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Symbol))
            .Returns(10);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Name))
            .Returns(0);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(summaries);

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = new GainLossResponse() });

        // Act
        var response = await _target.Search(searchText, max);

        // Assert
        response.Assets.Count().Should().Be(2);
        response.Assets.ToArray()[0].Symbol.Should().Be("PATH");
        response.Assets.ToArray()[1].Symbol.Should().Be("Z");
        response.MoreRecordsAvailable.Should().BeTrue();

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(6));
    }

    [Fact]
    public async Task Search_PrioritizesSymbolMatches()
    {
        // Arrange
        var searchText = "abc";
        var max = 3;

        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Id = 131,
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                },
                new AssetSummaryDto
                {
                    Id = 385,
                    Symbol = "SE",
                    Exchange = "NASDAQ",
                    Name = "Sea Limited",
                },
                new AssetSummaryDto
                {
                    Id = 222,
                    Symbol = "PATH",
                    Exchange = "NYSE",
                    Name = "UIPath",
                },
                new AssetSummaryDto
                {
                    Id = 104,
                    Symbol = "BOKU",
                    Exchange = "LSE",
                    Name = "Boku",
                },
            };

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Symbol))
            .Returns(1);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Name))
            .Returns(0);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Symbol))
            .Returns(10);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Name))
            .Returns(1);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Symbol))
            .Returns(2);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[2].Name))
            .Returns(2);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[3].Symbol))
            .Returns(10);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[3].Name))
            .Returns(2);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(summaries);

        _mockTimeSeriesService
            .Setup(e => e.Get(It.IsAny<string>()))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = new GainLossResponse() });

        // Act
        var response = await _target.Search(searchText, max);

        // Assert
        response.Assets.Count().Should().Be(3);
        response.Assets.ToArray()[0].Symbol.Should().Be("Z");
        response.Assets.ToArray()[1].Symbol.Should().Be("PATH");
        response.Assets.ToArray()[2].Symbol.Should().Be("SE");
        response.MoreRecordsAvailable.Should().BeTrue();

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(8));
    }

    [Fact]
    public async Task Search_MapsGainLoss()
    {
        // Arrange
        var searchText = "z";
        var max = 3;

        var summaries = new List<AssetSummaryDto>
            {
                new AssetSummaryDto
                {
                    Id = 131,
                    Symbol = "Z",
                    Exchange = "NASDAQ",
                    Name = "Zillow",
                    Key = "NASDAQ:Z",
                },
                new AssetSummaryDto
                {
                    Id = 385,
                    Symbol = "ZM",
                    Exchange = "NASDAQ",
                    Name = "Zoom",
                    Key = "NASDAQ:ZM",
                },
            };

        var gainLoss1 = new GainLossResponse
        {
            Last3Days = 123.5f,
        };

        var gainLoss2 = new GainLossResponse
        {
            Last2Weeks = 385.11f,
        };

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Symbol))
            .Returns(0);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[0].Name))
            .Returns(0);

        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Symbol))
            .Returns(1);
        _mockFuzzyMatch
            .Setup(e => e.Compute(searchText, summaries.ToArray()[1].Name))
            .Returns(1);

        _mockCache
            .Setup(e => e.Get(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()))
            .ReturnsAsync(summaries);

        _mockTimeSeriesService
            .Setup(e => e.Get(summaries.ToArray()[0].Key))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = gainLoss1 });
        _mockTimeSeriesService
            .Setup(e => e.Get(summaries.ToArray()[1].Key))
            .ReturnsAsync(new TimeSeriesSummary { GainLoss = gainLoss2 });

        // Act
        var response = await _target.Search(searchText, max);

        // Assert
        response.Assets.Count().Should().Be(2);

        response.Assets.ToArray()[0].Symbol.Should().Be("Z");
        response.Assets.ToArray()[0].GainLoss.Should().Be(gainLoss1);

        response.Assets.ToArray()[1].Symbol.Should().Be("ZM");
        response.Assets.ToArray()[1].GainLoss.Should().Be(gainLoss2);

        _mockCache.Verify(e => e.Get("AssetDtos", TimeSpan.FromDays(1), It.IsAny<Func<Task<IEnumerable<AssetSummaryDto>>>>()), Times.Once);
        _mockFuzzyMatch.Verify(e => e.Compute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        _mockTimeSeriesService.Verify(e => e.Get(It.IsAny<string>()), Times.Exactly(2));
    }

    //[Fact]
    //public async Task GetStarred_WhenNoEntries_ReturnsEmptyList()
    //{
    //    // Arrange
    //    var userId = 5;
    //    _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset>());

    //    // Act
    //    var response = await _target.GetStarred(userId);

    //    // Assert
    //    response.Any().Should().BeFalse();
    //    _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()), Times.Once);
    //}

    //[Fact]
    //public async Task GetStarred_WhenNone_ReturnsEmptyList()
    //{
    //    // Arrange
    //    var userId = 5;

    //    _mockUserStarredAssetRepository
    //        .Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset>());

    //    // Act
    //    var response = await _target.GetStarred(userId);

    //    // Assert
    //    response.Count().Should().Be(0);

    //    _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()), Times.Once);
    //    _mockTimeSeriesService.Verify(e => e.Get(It.IsAny<string>()), Times.Never);
    //}

    //[Fact]
    //public async Task GetStarred_ReturnsOrderedItems()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var userStarredAsset1 = new WatchlistAsset
    //    {
    //        UserId = userId,
    //        AssetId = 101,
    //        DisplayOrder = 51,
    //        Asset = new Asset
    //        {
    //            Id = 101,
    //            Symbol = "ZM",
    //            Exchange = "NASDAQ",
    //            Currency = "USD",
    //            Industry = "Tech",
    //            Name = "Zoom Video",
    //            Provider = "Alpaca",
    //        },
    //    };

    //    var gainLoss1 = new GainLossResponse
    //    {
    //        Last2Weeks = 101.3f,
    //    };

    //    _mockTimeSeriesService
    //        .Setup(e => e.Get("NASDAQ:ZM"))
    //        .ReturnsAsync(new TimeSeriesSummary { GainLoss = gainLoss1 });

    //    var userStarredAsset2 = new WatchlistAsset
    //    {
    //        UserId = userId,
    //        AssetId = 385,
    //        DisplayOrder = 1,
    //        Asset = new Asset
    //        {
    //            Id = 222,
    //            Symbol = "SE",
    //            Exchange = "NASDAQ",
    //            Name = "Sea Limited",
    //        },
    //    };

    //    var gainLoss2 = new GainLossResponse
    //    {
    //        Last3Days = 385.1337f,
    //    };

    //    _mockTimeSeriesService
    //        .Setup(e => e.Get("NASDAQ:SE"))
    //        .ReturnsAsync(new TimeSeriesSummary { GainLoss = gainLoss2 });

    //    _mockUserStarredAssetRepository
    //        .Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset> { userStarredAsset1, userStarredAsset2 });

    //    // Act
    //    var response = await _target.GetStarred(userId);

    //    // Assert
    //    response.Count().Should().Be(2);

    //    response.First().AssetId.Should().Be(userStarredAsset2.AssetId);

    //    _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()), Times.Once);
    //    _mockTimeSeriesService.Verify(e => e.Get("NASDAQ:ZM"), Times.Once);
    //    _mockTimeSeriesService.Verify(e => e.Get("NASDAQ:SE"), Times.Once);
    //}

    //[Fact]
    //public async Task GetStarred_MapsCorrectly()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var userStarredAsset = new WatchlistAsset
    //    {
    //        UserId = userId,
    //        AssetId = 101,
    //        DisplayOrder = 51,
    //        Asset = new Asset
    //        {
    //            Id = 101,
    //            Symbol = "ZM",
    //            Exchange = "NASDAQ",
    //            Currency = "USD",
    //            Industry = "Tech",
    //            Name = "Zoom Video",
    //            Provider = "Alpaca",
    //        },
    //    };

    //    var gainLoss = new GainLossResponse
    //    {
    //        Last2Weeks = 101.3f,
    //    };

    //    _mockUserStarredAssetRepository
    //        .Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset> { userStarredAsset });

    //    _mockTimeSeriesService
    //        .Setup(e => e.Get(It.IsAny<string>()))
    //        .ReturnsAsync(new TimeSeriesSummary { GainLoss = gainLoss });

    //    // Act
    //    var response = await _target.GetStarred(userId);

    //    // Assert
    //    response.Count().Should().Be(1);

    //    var starredAssetResponse = response.First();
    //    starredAssetResponse.AssetId.Should().Be(userStarredAsset.AssetId);
    //    starredAssetResponse.Symbol.Should().Be(userStarredAsset.Asset.Symbol);
    //    starredAssetResponse.Exchange.Should().Be(userStarredAsset.Asset.Exchange);
    //    starredAssetResponse.Key.Should().Be("NASDAQ:ZM");
    //    starredAssetResponse.Name.Should().Be(userStarredAsset.Asset.Name);
    //    starredAssetResponse.Industry.Should().Be(userStarredAsset.Asset.Industry);
    //    starredAssetResponse.Currency.Should().Be(userStarredAsset.Asset.Currency);
    //    starredAssetResponse.DisplayOrder.Should().Be(userStarredAsset.DisplayOrder);
    //    starredAssetResponse.GainLoss.Should().Be(gainLoss);

    //    _mockUserStarredAssetRepository.Verify(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()), Times.Once);
    //    _mockTimeSeriesService.Verify(e => e.Get("NASDAQ:ZM"), Times.Once);
    //}

    //[Fact]
    //public void Star_WhenRepositoryThrows_Throws()
    //{
    //    // Arrange
    //    var errorMessage = "big oof";
    //    _mockCoreUnitOfWork.Setup(e => e.SaveChanges())
    //        .ThrowsAsync(new Exception(errorMessage));

    //    // Act
    //    Func<Task> act = async () => await _target.Star(5, new StarAssetRequest());

    //    // Assert
    //    act.Should().Throw<Exception>().WithMessage(errorMessage);
    //}

    //[Fact]
    //public async Task Star_SavesCorrectData()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var request = new StarAssetRequest
    //    {
    //        AssetId = 385,
    //        DisplayOrder = 122,
    //    };

    //    // Act
    //    await _target.Star(userId, request);

    //    // Assert
    //    _mockUserStarredAssetRepository.Verify(x => x.Add(
    //        It.Is<WatchlistAsset>(e => e.AssetId == request.AssetId && e.UserId == userId && e.DisplayOrder == request.DisplayOrder)),
    //            Times.Once);
    //    _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    //}

    //[Fact]
    //public void UpdateDisplaySortOrder_WhenEntryNotFound_Throws()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var request = new StarAssetRequest
    //    {
    //        AssetId = 385,
    //        DisplayOrder = 51,
    //    };

    //    _mockUserStarredAssetRepository.Setup(e => e.FindWithTracking(It.IsAny<Expression<Func<WatchlistAsset, bool>>>()))
    //        .ReturnsAsync(new List<WatchlistAsset>());

    //    // Act
    //    Func<Task> act = async () => await _target.UpdateStarDisplayOrder(userId, request);

    //    // Assert
    //    act.Should().Throw<Exception>();
    //    _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
    //}

    //[Fact]
    //public async Task UpdateDisplaySortOrder_UpdatesOrder()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var assetId = 385;
    //    var userStarredAsset = new WatchlistAsset
    //    {
    //        UserId = userId,
    //        AssetId = assetId,
    //        DisplayOrder = 51,
    //    };

    //    var request = new StarAssetRequest
    //    {
    //        AssetId = assetId,
    //        DisplayOrder = 222,
    //    };

    //    _mockUserStarredAssetRepository.Setup(e => e.FindWithTracking(It.IsAny<Expression<Func<WatchlistAsset, bool>>>()))
    //        .ReturnsAsync(new List<WatchlistAsset> { userStarredAsset });

    //    // Act
    //    await _target.UpdateStarDisplayOrder(userId, request);

    //    // Assert
    //    userStarredAsset.DisplayOrder.Should().Be(request.DisplayOrder);
    //    _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    //}

    //[Fact]
    //public void Unstar_WhenEntryNotFound_Throws()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var assetId = 385;

    //    _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset>());

    //    // Act
    //    Func<Task> act = async () => await _target.Unstar(userId, assetId);

    //    // Assert
    //    act.Should().Throw<Exception>();
    //    _mockUserStarredAssetRepository.Verify(x => x.Delete(It.IsAny<WatchlistAsset>()), Times.Never);
    //    _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Never);
    //}

    //[Fact]
    //public async Task Unstar_RemovesCorrectEntry()
    //{
    //    // Arrange
    //    var userId = 5;
    //    var userStarredAsset = new WatchlistAsset
    //    {
    //        UserId = userId,
    //        AssetId = 385,
    //        DisplayOrder = 51,
    //    };

    //    _mockUserStarredAssetRepository.Setup(e => e.Find(It.IsAny<Expression<Func<WatchlistAsset, bool>>>(), It.IsAny<string>()))
    //        .ReturnsAsync(new List<WatchlistAsset> { userStarredAsset });

    //    // Act
    //    await _target.Unstar(userId, userStarredAsset.AssetId);

    //    // Assert
    //    _mockUserStarredAssetRepository.Verify(x => x.Delete(
    //        It.Is<WatchlistAsset>(e => e.AssetId == userStarredAsset.AssetId && e.UserId == userId && e.DisplayOrder == userStarredAsset.DisplayOrder)),
    //            Times.Once);
    //    _mockCoreUnitOfWork.Verify(e => e.SaveChanges(), Times.Once);
    //}
}
