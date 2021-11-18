using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Investager.Infrastructure.UnitTests.Services;

public class AlpacaServiceUnitTests
{
    private static readonly DateTime UtcNow = new DateTime(2021, 04, 11, 12, 16, 33, DateTimeKind.Utc);

    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();

    private readonly AlpacaService _target;

    public AlpacaServiceUnitTests()
    {
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://www.fake.com")
        };

        _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(UtcNow);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _target = new AlpacaService(
            mockHttpClientFactory.Object,
            _mockTimeHelper.Object);
    }

    [Fact]
    public async Task GetAssets_ReturnsCorrectData()
    {
        // Arrange
        var responseContent = await File.ReadAllTextAsync("TestData/AlpacaGetAssetsExample.json");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _target.GetAssets();

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    e => e.RequestUri.ToString() == "http://www.fake.com/v2/assets"),
                ItExpr.IsAny<CancellationToken>());

        var assets = result.ToArray();

        assets.Length.Should().Be(4);

        assets.All(e => e.Currency == "USD").Should().BeTrue();
        assets.All(e => e.Provider == DataProviders.Alpaca).Should().BeTrue();

        assets[0].Symbol.Should().Be("NES");
        assets[0].Exchange.Should().Be("AMEX");

        assets[1].Symbol.Should().Be("NETE");
        assets[1].Exchange.Should().Be("NASDAQ");

        assets[2].Symbol.Should().Be("NETI");
        assets[2].Exchange.Should().Be("NYSE");

        assets[3].Symbol.Should().Be("NETL");
        assets[3].Exchange.Should().Be("ARCA");
    }

    [Fact]
    public async Task GetAssets_WhenAlpacaCallFails_Throws()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.TooManyRequests,
            Content = new StringContent("Retry back later."),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        Func<Task> act = async () => await _target.GetAssets();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetRecentPoints_WhenAlpacaCallFails_Throws()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.TooManyRequests,
            Content = new StringContent("Retry back later."),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var request = new UpdateAssetDataRequest
        {
            Symbol = "SE",
            Exchange = "NASDAQ",
            Key = "NASDAQ:SE",
        };

        // Act
        Func<Task> act = async () => await _target.GetRecentPoints(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetRecentPoints_WhenNoPreviousData_AlpacaUrl_IsBuiltCorrectly()
    {
        // Arrange
        var responseContent = File.ReadAllText("TestData/AlpacaGetStockDataExample.json");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var request = new UpdateAssetDataRequest
        {
            Symbol = "SE",
            Exchange = "NASDAQ",
            Key = "NASDAQ:SE",
        };

        // Act
        await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    e => e.RequestUri != null
                        && e.RequestUri.ToString()
                            .Contains($"stocks/{request.Symbol}/bars?start=2016-04-11T12:17:33.0000000Z" +
                                $"&end=2021-04-11T11:16:33.0000000Z&timeframe=1Day&limit=10000")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPoints_WithPreviousData_AlpacaUrl_IsBuiltCorrectly()
    {
        // Arrange
        var responseContent = File.ReadAllText("TestData/AlpacaGetStockDataExample.json");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var request = new UpdateAssetDataRequest
        {
            Symbol = "SE",
            Exchange = "NASDAQ",
            Key = "NASDAQ:SE",
            LatestPointTime = new DateTime(2021, 02, 02, 13, 37, 0, DateTimeKind.Utc),
        };

        // Act
        await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    e => e.RequestUri != null
                        && e.RequestUri.ToString()
                            .Contains($"stocks/{request.Symbol}/bars?start=2021-02-02T13:37:00.0000000Z" +
                                $"&end=2021-04-11T11:16:33.0000000Z&timeframe=1Day&limit=10000")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPoints_ReturnsExpectedDataPoints()
    {
        // Arrange
        var responseContent = File.ReadAllText("TestData/AlpacaGetStockDataExample.json");
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var request = new UpdateAssetDataRequest
        {
            Symbol = "SE",
            Exchange = "NASDAQ",
            Key = "NASDAQ:SE",
            LatestPointTime = new DateTime(2014, 10, 10, 13, 37, 00, DateTimeKind.Utc),
        };

        // Act
        var recentPoints = await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        e => e.RequestUri != null
                            && e.RequestUri.ToString()
                                .Contains($"stocks/{request.Symbol}/bars?start=2016-04-11T12:17:33.0000000Z" +
                                    $"&end=2021-04-11T11:16:33.0000000Z&timeframe=1Day&limit=10000")),
                    ItExpr.IsAny<CancellationToken>());

        var points = recentPoints.ToArray();

        points.Length.Should().Be(5);

        points.All(e => e.Key == "NASDAQ:SE").Should().BeTrue();
        points.All(e => e.Time > request.LatestPointTime).Should().BeTrue();

        points[0].Time.Should().Be(new DateTime(2016, 04, 12, 04, 00, 00, DateTimeKind.Utc));
        points[0].Value.Should().Be(12.81F);

        points[1].Time.Should().Be(new DateTime(2016, 04, 13, 04, 00, 00, DateTimeKind.Utc));
        points[1].Value.Should().Be(13.06F);

        points[2].Time.Should().Be(new DateTime(2016, 04, 14, 04, 00, 00, DateTimeKind.Utc));
        points[2].Value.Should().Be(13.09F);

        points[3].Time.Should().Be(new DateTime(2016, 04, 15, 04, 00, 00, DateTimeKind.Utc));
        points[3].Value.Should().Be(12.94F);

        points[4].Time.Should().Be(new DateTime(2016, 04, 18, 04, 00, 00, DateTimeKind.Utc));
        points[4].Value.Should().Be(13.25F);
    }
}
