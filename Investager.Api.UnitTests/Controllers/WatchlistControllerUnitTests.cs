using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
    public class WatchlistControllerUnitTests
    {
        private readonly Mock<IWatchlistService> _mockWatchlistService = new Mock<IWatchlistService>();
        private readonly HttpContext _httpContext = new DefaultHttpContext();

        private readonly WatchlistController _target;

        public WatchlistControllerUnitTests()
        {
            _target = new WatchlistController(
                _mockWatchlistService.Object);

            _target.ControllerContext.HttpContext = _httpContext;
        }

        [Fact]
        public async Task GetWatchlists_ReturnsDataFromService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var watchlists = new List<WatchlistLightResponse>
            {
                new WatchlistLightResponse
                {
                    Id = 1,
                    Name = "Default",
                    DisplayOrder = 101,
                },
                new WatchlistLightResponse
                {
                    Id = 104,
                    Name = "Pantelimon",
                    DisplayOrder = 222,
                },
            };

            _mockWatchlistService
                .Setup(e => e.GetForUser(It.IsAny<int>()))
                .ReturnsAsync(watchlists);

            // Act
            var response = await _target.GetWatchlists();

            // Assert
            _mockWatchlistService.Verify(e => e.GetForUser(userId), Times.Once);

            response.Value.Should().BeEquivalentTo(watchlists);
        }

        [Fact]
        public async Task GetWatchlist_ReturnsDataFromService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;

            var watchlist = new WatchlistResponse
            {
                Id = watchlistId,
                Assets = new List<WatchedAssetResponse>
                {
                    new WatchedAssetResponse
                    {
                        AssetId = 333,
                        Currency = "USD",
                        Symbol = "ZM",
                        DisplayOrder = 5,
                    },
                },
                CurrencyPairs = new List<WatchedCurrencyPairResponse>(),
            };

            _mockWatchlistService
                .Setup(e => e.GetById(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(watchlist);

            // Act
            var response = await _target.GetWatchlist(watchlistId);

            // Assert
            _mockWatchlistService.Verify(e => e.GetById(userId, watchlistId), Times.Once);

            response.Value.Should().BeEquivalentTo(watchlist);
        }

        [Fact]
        public async Task WatchAsset_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var assetId = 139;
            var body = new DisplayOrderBody
            {
                DisplayOrder = 601,
            };

            // Act
            var response = await _target.WatchAsset(watchlistId, assetId, body);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.WatchAsset(It.Is<WatchAssetRequest>(e => e.UserId == userId
                    && e.WatchlistId == watchlistId
                    && e.AssetId == assetId
                    && e.DisplayOrder == body.DisplayOrder)),
                Times.Once);
        }

        [Fact]
        public async Task WatchCurrencyPair_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var currencyPairId = 139;
            var body = new DisplayOrderBody
            {
                DisplayOrder = 601,
            };

            // Act
            var response = await _target.WatchCurrencyPair(watchlistId, currencyPairId, body);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.WatchCurrencyPair(It.Is<WatchCurrencyPairRequest>(e => e.UserId == userId
                    && e.WatchlistId == watchlistId
                    && e.CurrencyPairId == currencyPairId
                    && e.DisplayOrder == body.DisplayOrder)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateDisplayOrder_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var body = new DisplayOrderBody
            {
                DisplayOrder = 601,
            };

            // Act
            var response = await _target.UpdateDisplayOrder(watchlistId, body);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.UpdateDisplayOrder(userId, watchlistId, body.DisplayOrder),
                Times.Once);
        }

        [Fact]
        public async Task UpdateName_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var body = new UpdateWatchlistNameBody
            {
                Name = "Delaco",
            };

            // Act
            var response = await _target.UpdateName(watchlistId, body);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.UpdateName(userId, watchlistId, body.Name),
                Times.Once);
        }

        [Fact]
        public async Task UnwatchAsset_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var assetId = 601;

            // Act
            var response = await _target.UnwatchAsset(watchlistId, assetId);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.UnwatchAsset(userId, watchlistId, assetId),
                Times.Once);
        }

        [Fact]
        public async Task UnwatchCurrencyPair_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;
            var currencyPairId = 601;

            // Act
            var response = await _target.UnwatchCurrencyPair(watchlistId, currencyPairId);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.UnwatchCurrencyPair(userId, watchlistId, currencyPairId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteWatchlist_CallsService()
        {
            // Arrange
            var userId = 385;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();
            var watchlistId = 104;

            // Act
            var response = await _target.Delete(watchlistId);

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);

            _mockWatchlistService.Verify(
                e => e.Delete(userId, watchlistId),
                Times.Once);
        }
    }
}
