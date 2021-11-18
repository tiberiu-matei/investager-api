using FluentAssertions;
using Investager.Api.Controllers;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Api.UnitTests.Controllers;

public class AssetControllerUnitTests
{
    private readonly Mock<IAssetService> _mockAssetService = new Mock<IAssetService>();
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    private readonly AssetController _target;

    public AssetControllerUnitTests()
    {
        _target = new AssetController(
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

        _mockAssetService
            .Setup(e => e.Search(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(assetSearchResponse);

        // Act
        var response = await _target.Search(searchText, max);

        // Assert
        response.Value.Should().Be(assetSearchResponse);

        _mockAssetService.Verify(e => e.Search(searchText, max), Times.Once);
    }
}
