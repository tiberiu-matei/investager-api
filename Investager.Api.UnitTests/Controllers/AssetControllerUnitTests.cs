using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers
{
    public class AssetControllerUnitTests
    {
        private readonly Mock<IDataProviderServiceFactory> _mockDataProviderServiceFactory = new Mock<IDataProviderServiceFactory>();
        private readonly Mock<IDataCollectionServiceFactory> _mockDataCollectionServiceFactory = new Mock<IDataCollectionServiceFactory>();
        private readonly Mock<IAssetService> _mockAssetService = new Mock<IAssetService>();
        private readonly HttpContext _httpContext = new DefaultHttpContext();

        private readonly AssetController _target;

        public AssetControllerUnitTests()
        {
            _target = new AssetController(
                _mockDataProviderServiceFactory.Object,
                _mockDataCollectionServiceFactory.Object,
                _mockAssetService.Object);

            _target.ControllerContext.HttpContext = _httpContext;
        }

        [Fact]
        public async Task Search_ReturnsExpectedData()
        {
            // Arrange
            var assetSearchResponse = new AssetSearchResponse
            {
                Assets = new List<AssetSummaryDto>
                {
                    new AssetSummaryDto
                    {
                        Symbol = "NVDA",
                        Exchange = "NASDAQ",
                        Name = "Nvidia",
                    },
                },
                MoreRecordsAvailable = true,
            };

            var searchText = "abc";
            var max = 11;

            _mockAssetService.Setup(e => e.Search(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(assetSearchResponse);

            // Act
            var response = await _target.Search(searchText, max);

            // Assert
            _mockAssetService.Verify(e => e.Search(searchText, max), Times.Once);

            var result = response as OkObjectResult;
            var value = result.Value as AssetSearchResponse;
            result.StatusCode.Should().Be(200);
            value.Should().Be(assetSearchResponse);
        }

        [Fact]
        public async Task GetStarred_ReturnsExpectedDtos()
        {
            // Arrange
            var userId = 5;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var dto = new StarredAssetResponse
            {
                AssetId = 385,
                DisplayOrder = 122,
            };

            _mockAssetService.Setup(e => e.GetStarred(userId)).ReturnsAsync(new List<StarredAssetResponse> { dto });

            // Act
            var response = await _target.GetStarred();

            // Assert
            _mockAssetService.Verify(e => e.GetStarred(userId), Times.Once);

            var result = response as OkObjectResult;
            var value = result.Value as IEnumerable<StarredAssetResponse>;
            result.StatusCode.Should().Be(200);
            value.Count().Should().Be(1);
            value.First().Should().Be(dto);
        }

        [Fact]
        public async Task Scan_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataProviderService>();
            _mockDataProviderServiceFactory.Setup(e => e.CreateService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = await _target.Scan();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.ScanAssets(), Times.Once);
        }

        [Fact]
        public void Start_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataCollectionService>();
            _mockDataCollectionServiceFactory.Setup(e => e.GetService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = _target.Start();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.Start(), Times.Once);
        }

        [Fact]
        public void Stop_InvokesCorrectMethod()
        {
            // Arrange
            var serviceMock = new Mock<IDataCollectionService>();
            _mockDataCollectionServiceFactory.Setup(e => e.GetService(It.IsAny<string>())).Returns(serviceMock.Object);

            // Act
            var response = _target.Stop();

            // Assert
            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
            serviceMock.Verify(e => e.Stop(), Times.Once);
        }

        [Fact]
        public async Task Star_CallsService()
        {
            // Arrange
            var userId = 5;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var request = new StarAssetRequest
            {
                AssetId = 385,
                DisplayOrder = 122,
            };

            // Act
            var response = await _target.Star(request);

            // Assert
            _mockAssetService.Verify(e => e.Star(userId, request), Times.Once);

            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task UpdateStarDisplayOrder_CallsService()
        {
            // Arrange
            var userId = 5;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var request = new StarAssetRequest
            {
                AssetId = 385,
                DisplayOrder = 122,
            };

            // Act
            var response = await _target.UpdateStarDisplayOrder(request);

            // Assert
            _mockAssetService.Verify(e => e.UpdateStarDisplayOrder(userId, request), Times.Once);

            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Unstar_CallsService()
        {
            // Arrange
            var userId = 5;
            _httpContext.Items[HttpContextKeys.UserId] = userId.ToString();

            var assetId = 385;

            // Act
            var response = await _target.Unstar(assetId);

            // Assert
            _mockAssetService.Verify(e => e.Unstar(userId, assetId), Times.Once);

            var result = response as NoContentResult;
            result.StatusCode.Should().Be(204);
        }
    }
}
