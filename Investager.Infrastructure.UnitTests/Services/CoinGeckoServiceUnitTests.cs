using FluentAssertions;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Infrastructure.Services;
using Investager.Infrastructure.Settings;
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

public class CoinGeckoServiceUnitTests
{
    private static readonly DateTime UtcNow = new DateTime(2021, 11, 15, 23, 37, 00, DateTimeKind.Utc);

    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    private readonly Mock<ITimeHelper> _mockTimeHelper = new Mock<ITimeHelper>();
    private readonly CoinGeckoSettings _coinGeckoSettings = new CoinGeckoSettings();

    private readonly CoinGeckoService _target;

    public CoinGeckoServiceUnitTests()
    {
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://www.fake.com")
        };

        _mockTimeHelper.Setup(e => e.GetUtcNow()).Returns(UtcNow);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _coinGeckoSettings.DataQueryInterval = TimeSpan.FromMilliseconds(1);

        _target = new CoinGeckoService(
            mockHttpClientFactory.Object,
            _mockTimeHelper.Object,
            _coinGeckoSettings);
    }

    [Fact]
    public async Task GetPairs_ReturnsPairs_AboveTheCapTreshold()
    {
        // Arrange
        var coinsResponseContent = await File.ReadAllTextAsync("TestData/CoinGeckoCoinsExample.json");
        var coinsResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(coinsResponseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().Contains("v3/coins/list")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(coinsResponse);

        await SetupCardanoChartCall();
        await SetupEthereumChartCall();

        var daggerResponseContent = await File.ReadAllTextAsync("TestData/CoinGeckoChartXdagUsd91Days.json");
        var daggerResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(daggerResponseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().Contains("v3/coins/dagger/market_chart?vs_currency=usd")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(daggerResponse);

        // Act
        var result = await _target.GetPairs();

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(4),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        var pairs = result.ToArray();

        pairs.Length.Should().Be(2);

        pairs.All(e => e.HasTimeData).Should().BeTrue();
        pairs.All(e => e.Provider == DataProviders.CoinGecko).Should().BeTrue();

        pairs[0].FirstCurrency.Code.Should().Be("ada");
        pairs[0].FirstCurrency.Name.Should().Be("Cardano");
        pairs[0].FirstCurrency.ProviderId.Should().Be("cardano");
        pairs[0].SecondCurrency.Code.Should().Be("usd");
        pairs[0].SecondCurrency.Name.Should().Be("American Dollar");
        pairs[0].SecondCurrency.ProviderId.Should().Be("usd");

        pairs[1].FirstCurrency.Code.Should().Be("eth");
        pairs[1].FirstCurrency.Name.Should().Be("Ethereum");
        pairs[1].FirstCurrency.ProviderId.Should().Be("ethereum");
        pairs[1].SecondCurrency.Code.Should().Be("usd");
        pairs[1].SecondCurrency.Name.Should().Be("American Dollar");
        pairs[1].SecondCurrency.ProviderId.Should().Be("usd");
    }

    [Fact]
    public async Task GetPairs_WhenCoinGeckoResponseEmpty_ReturnsEmptyList()
    {
        // Arrange
        var coinsResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[]"),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().Contains("v3/coins/list")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(coinsResponse);

        // Act
        var result = await _target.GetPairs();

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        result.Any().Should().BeFalse();
    }

    [Fact]
    public async Task GetPairs_WhenCoinGeckoCallFails_Throws()
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
        Func<Task> act = async () => await _target.GetPairs();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetRecentPoints_WhenNoLatestPointExists_AsksForMaxPoints()
    {
        // Arrange
        await SetupEthereumChartCall();

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "ethereum",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
        };

        // Act
        await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().EndsWith("ethereum/market_chart?vs_currency=usd&days=max")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPoints_WhenLatestPoint150DaysAgo_AsksFor150Days()
    {
        // Arrange
        await SetupEthereumChartCall();

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "ethereum",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
            LatestPointTime = UtcNow - TimeSpan.FromDays(150),
        };

        // Act
        await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().EndsWith("ethereum/market_chart?vs_currency=usd&days=150")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPoints_WhenLatestPointLessThan91DaysAgo_AsksFor91Days()
    {
        // Arrange
        await SetupEthereumChartCall();

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "ethereum",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
            LatestPointTime = UtcNow - TimeSpan.FromDays(13),
        };

        // Act
        await _target.GetRecentPoints(request);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().EndsWith("ethereum/market_chart?vs_currency=usd&days=91")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPoints_WhenNoLatestPointTime_ReturnsAllPoints()
    {
        // Arrange
        await SetupEthereumChartCall();

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "ethereum",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
        };

        // Act
        var recentPoints = await _target.GetRecentPoints(request);

        // Assert
        recentPoints.Count().Should().Be(92);
    }

    [Fact]
    public async Task GetRecentPoints_OnlyReturnsPoints_LaterThanLatestPointTime()
    {
        // Arrange
        await SetupEthereumChartCall();

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "ethereum",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
            LatestPointTime = UtcNow - TimeSpan.FromDays(3),
        };

        // Act
        var recentPoints = await _target.GetRecentPoints(request);

        // Assert
        recentPoints.Count().Should().Be(4);
        recentPoints.All(e => e.Time > request.LatestPointTime).Should().BeTrue();
        recentPoints.All(e => e.Key == request.Key).Should().BeTrue();
        recentPoints.Single(e => e.Value == 4587.499707364769f);
        recentPoints.Single(e => e.Value == 4652.947394238871f);
        recentPoints.Single(e => e.Value == 4666.498498194288f);
        recentPoints.Single(e => e.Value == 4685.10635551733f);
    }

    [Fact]
    public async Task GetRecentPoints_WhenCoinGeckoCallFails_Throws()
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

        var request = new UpdateCurrencyPairDataRequest
        {
            FirstCurrencyProviderId = "eth",
            SecondCurrencyProviderId = "usd",
            Key = "eth:usd",
        };

        // Act
        Func<Task> act = async () => await _target.GetRecentPoints(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private async Task SetupCardanoChartCall()
    {
        var cardanoResponseContent = await File.ReadAllTextAsync("TestData/CoinGeckoChartAdaUsd91Days.json");
        var cardanoResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(cardanoResponseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().Contains("v3/coins/cardano/market_chart?vs_currency=usd")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(cardanoResponse);
    }

    private async Task SetupEthereumChartCall()
    {
        var ethereumResponseContent = await File.ReadAllTextAsync("TestData/CoinGeckoChartEthUsd91Days.json");
        var ethereumResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(ethereumResponseContent),
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(e => e.RequestUri.ToString().Contains("v3/coins/ethereum/market_chart?vs_currency=usd")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(ethereumResponse);
    }
}
